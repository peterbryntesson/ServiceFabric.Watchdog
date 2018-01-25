using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using ServiceFabric.Watchdog.QueryObjects;
using ServiceFabric.Watchdog.RuleEngine.Model;
using ServiceFabric.Watchdog.RuleEngine.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ServiceFabric.Watchdog
{
    public class RuleWatchdog
    {
        protected System.Timers.Timer _timer = new System.Timers.Timer();
        protected SFRoot _root = new SFRoot(new System.Fabric.FabricClient());
        protected IReliableStateManager _stateManager;
        protected CancellationTokenSource _tokenSource = new CancellationTokenSource();
        protected List<Rule> Rules { get; } = new List<Rule>();
        public List<TriggerItem> CurrentTriggerItems { get; } = new List<TriggerItem>();

        public RuleWatchdog(IReliableStateManager stateManager = null)
        {
            _stateManager = stateManager;
        }

        public async Task Load()
        {
            if (_stateManager != null)
            {
                var rules = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Rule>>("Rules");
                using (var tx = _stateManager.CreateTransaction())
                {
                    var token = _tokenSource.Token;
                    var enumerator = (await rules.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync(token))
                    {
                        lock (Rules)
                        {
                            Rules.Add(enumerator.Current.Value);
                        }
                    }
                }
            }
        }

        public void Start(TimeSpan checkInterval)
        {
            _timer.Interval = checkInterval.TotalMilliseconds;
            _timer.Elapsed += OnCheckRules;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public async Task AddRule(Rule rule)
        {
            lock (Rules)
            {
                var ruleInList = Rules.Where(r => r.Name == rule.Name).FirstOrDefault();
                if (ruleInList != null)
                    Rules.Remove(ruleInList);
                Rules.Add(rule);
            }
            await SaveRule(rule);
        }

        public async Task RemoveRule(Rule rule)
        {
            Rule ruleInList = null;
            lock (Rules)
            {
                ruleInList = Rules.Where(r => r.Name == rule.Name).FirstOrDefault();
                if (ruleInList != null)
                    Rules.Remove(ruleInList);
            }

            if (ruleInList != null && _stateManager != null)
            {
                var rules = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Rule>>("Rules");
                using (var tx = _stateManager.CreateTransaction())
                {
                    await rules.TryRemoveAsync(tx, rule.Name);
                    await tx.CommitAsync();
                }
            }
        }

        public async Task RemoveAllRules()
        {
            lock (Rules)
            {
                Rules.Clear();
            }
            if (_stateManager != null)
            {
                var rules = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Rule>>("Rules");
                await rules.ClearAsync(new TimeSpan(0, 0, 30), _tokenSource.Token);
            }
        }

        private async Task SaveRule(Rule rule)
        {
            if (_stateManager != null)
            {
                var rules = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Rule>>("Rules");
                using (var tx = _stateManager.CreateTransaction())
                {
                    await rules.AddOrUpdateAsync(tx, rule.Name, rule, (key, value) => value);
                    await tx.CommitAsync();
                }
            }
        }
        private async void OnCheckRules(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            try
            {
                await _root.Refresh(true);
            }
            catch (Exception)
            {
                _timer.Start();
                return;
            }

            // gather metrics from SF
            var newTriggerItems = GatherMetricsFromSF();

            foreach (var rule in Rules)
            {
                rule.Execute(newTriggerItems);
                rule.ExecuteAction();
                await SaveRule(rule);
            }

            CurrentTriggerItems.Clear();
            CurrentTriggerItems.AddRange(newTriggerItems);

            _timer.Start();
        }

        private List<TriggerItem> GatherMetricsFromSF()
        {
            var triggerItems = new List<TriggerItem>();
            foreach (var application in _root.Applications)
            {
                foreach (var service in application.Services)
                {
                    foreach (var partition in service.Partitions)
                    {
                        foreach (var replica in partition.Replicas)
                        {
                            var triggerItem = new TriggerItem()
                            {
                                Application = application,
                                Service = service,
                                Partition = partition,
                                Replica = replica
                            };
                            foreach (var metricReport in replica.DeploymentDetails.ReportedLoad)
                            {
                                triggerItem.Metrics.Add(new TriggerItemMetric()
                                {
                                    Name = metricReport.Name,
                                    Value = metricReport.Value,
                                    LastReportedUtc = metricReport.LastReportedUtc
                                });
                            }
                            if (triggerItem.Metrics.Any())
                            {
                                triggerItems.Add(triggerItem);
                                WatchdogEventSource.Current.TriggerItemReceived(triggerItem.ToString());
                            }
                        }
                    }
                }
            }

            return triggerItems;
        }
    }
}
