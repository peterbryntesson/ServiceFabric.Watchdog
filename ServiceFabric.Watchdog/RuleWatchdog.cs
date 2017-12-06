using ServiceFabric.Watchdog.QueryObjects;
using ServiceFabric.Watchdog.RuleEngine.Model;
using ServiceFabric.Watchdog.RuleEngine.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace ServiceFabric.Watchdog
{
    public class RuleWatchdog
    {
        protected System.Timers.Timer _timer = new System.Timers.Timer();
        protected SFRoot _root = new SFRoot(new System.Fabric.FabricClient());

        public List<Rule> Rules { get; } = new List<Rule>();
        public List<TriggerItem> CurrentTriggerItems { get; } = new List<TriggerItem>();

        public void Start(TimeSpan checkInterval)
        {
            _timer.Interval = checkInterval.TotalMilliseconds;
            _timer.Elapsed += OnCheckRules;
            _timer.Start();
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
            }

            CurrentTriggerItems.Clear();
            CurrentTriggerItems.AddRange(newTriggerItems);

            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
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
