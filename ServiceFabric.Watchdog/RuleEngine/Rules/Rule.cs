using ServiceFabric.Watchdog.RuleEngine.Actions;
using ServiceFabric.Watchdog.RuleEngine.Expressions;
using ServiceFabric.Watchdog.RuleEngine.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ServiceFabric.Watchdog.RuleEngine.Rules
{
    public class Rule
    {
        public StringExpression RuleFilter { get; set; }
        public IntExpression Expression { get; set; }
        public TimeSpan TriggerPeriod { get; set; }
        public TimeSpan GracePeriod { get; set; }
        public RuleAction TriggerAction { get; set; }
        public List<RuleActionItem> ActionHistory { get; } = new List<RuleActionItem>();
        public bool AggregateData { get; set; } = true;

        private DateTime? _firstTriggered;
        private TriggerItem _triggeringItem;

        public void Execute(List<TriggerItem> triggerItems)
        {
            var filteredTriggerItems = new List<TriggerItem>();
            TriggerItem defaultTriggerItem = null;

            if (!triggerItems.Any())
                return;

            if (Expression == null)
            {
                defaultTriggerItem = triggerItems.First();
                WatchdogEventSource.Current.RuleFired(this.ToString(defaultTriggerItem), defaultTriggerItem.ToString(), false);
                return;
            }

            if (RuleFilter != null)
            {
                foreach (var triggerItem in triggerItems)
                {
                    var result = (RuleFilter.Execute(triggerItem) == "1" ? true : false);
                    if (result)
                        filteredTriggerItems.Add(triggerItem);
                }

                if (!filteredTriggerItems.Any())
                {
                    defaultTriggerItem = triggerItems.First();
                    _firstTriggered = null;
                    _triggeringItem = null;
                    WatchdogEventSource.Current.RuleFired(this.ToString(defaultTriggerItem), defaultTriggerItem.ToString(), false);
                    return;
                }
            }

            if (AggregateData)
                filteredTriggerItems = CreateAggregatedData(filteredTriggerItems);

            foreach (var filteredTriggerItem in filteredTriggerItems)
            {
                if (Expression.Execute(filteredTriggerItem) != 0)
                {
                    if (_firstTriggered == null)
                        _firstTriggered = DateTime.UtcNow;
                    _triggeringItem = filteredTriggerItem;
                    WatchdogEventSource.Current.RuleFired(this.ToString(filteredTriggerItem), filteredTriggerItem.ToString(), true);
                    return;
                }
            }

            defaultTriggerItem = triggerItems.First();
            _firstTriggered = null;
            _triggeringItem = null;
            WatchdogEventSource.Current.RuleFired(this.ToString(defaultTriggerItem), defaultTriggerItem.ToString(), false);
        }

        public void ExecuteAction()
        {
            if (_firstTriggered != null && (DateTime.UtcNow - TriggerPeriod) > _firstTriggered)
            {
                //  make sure no recent action was taken on this
                foreach (var actionItem in ActionHistory)
                {
                    if (actionItem.TriggerItem.Application.Properties.ApplicationName == _triggeringItem.Application.Properties.ApplicationName &&
                        actionItem.TriggerItem.Service.Properties.ServiceName == _triggeringItem.Service.Properties.ServiceName &&
                        (DateTime.UtcNow - GracePeriod) < actionItem.TimeTriggeredUtc)
                        // we should do any new action yet - we have a grace period
                        return;
                }

                TriggerAction.Execute(_triggeringItem);
                WatchdogEventSource.Current.ActionFired(TriggerAction.GetType().FullName, this.ToString(_triggeringItem), _triggeringItem.ToString());

                ActionHistory.Add(new RuleActionItem()
                {
                    TimeTriggeredUtc = DateTime.UtcNow,
                    TriggerItem = _triggeringItem,
                    TriggerAction = TriggerAction
                });
            }
        }

        public string ToString(TriggerItem triggerItem)
        {
            return $"RuleFilter={RuleFilter.ToString(triggerItem)}; Expression={Expression.ToString(triggerItem)}; TriggerPeriod={TriggerPeriod.ToString()}; " +
                $"GracePeriod={GracePeriod.ToString()}; Action={TriggerAction.GetType().FullName}";
        }

        private List<TriggerItem> CreateAggregatedData(List<TriggerItem> triggerItems)
        {
            var aggregatedTriggerItems = new Dictionary<string, AggregatedTriggerItem>();
            foreach (var triggerItem in triggerItems)
            {
                string key = $"{triggerItem.Application.Properties.ApplicationName}-{triggerItem.Service.Properties.ServiceName}";
                AggregatedTriggerItem aggregatedTriggerItem = null;
                if (!aggregatedTriggerItems.TryGetValue(key, out aggregatedTriggerItem))
                {
                    aggregatedTriggerItem = new AggregatedTriggerItem()
                    {
                        Application = triggerItem.Application,
                        Service = triggerItem.Service,
                        Partition = triggerItem.Partition,
                        Replica = triggerItem.Replica
                    };
                }

                foreach (var metric in triggerItem.Metrics)
                {
                    AggregatedTriggerItemValue aggregatedValue = null;
                    if (aggregatedTriggerItem.TriggerItems.TryGetValue(metric.Name, out aggregatedValue))
                    {
                        aggregatedValue.Count++;
                        aggregatedValue.Sum += metric.Value;
                    }
                    else
                    {
                        aggregatedValue = new AggregatedTriggerItemValue()
                        {
                            Count = 1,
                            Sum = metric.Value
                        };
                    }
                    aggregatedTriggerItem.TriggerItems[metric.Name] = aggregatedValue;
                }
                aggregatedTriggerItems[key] = aggregatedTriggerItem;
            }

            triggerItems = new List<TriggerItem>();
            foreach (var aggregatedTriggerItem in aggregatedTriggerItems.Values)
            {
                // make an "aggregated" trigger item to run the rule on
                var triggerItem = new TriggerItem()
                {
                    Application = aggregatedTriggerItem.Application,
                    Service = aggregatedTriggerItem.Service,
                    Partition = aggregatedTriggerItem.Partition,
                    Replica = aggregatedTriggerItem.Replica
                };
                foreach (var aggregatedTriggerItemValuePair in aggregatedTriggerItem.TriggerItems)
                {
                    triggerItem.Metrics.Add(new TriggerItemMetric()
                    {
                        Name = aggregatedTriggerItemValuePair.Key,
                        Value = (int)((double)aggregatedTriggerItemValuePair.Value.Sum / (double)aggregatedTriggerItemValuePair.Value.Count),
                        LastReportedUtc = DateTime.UtcNow
                    });
                }
                triggerItems.Add(triggerItem);
            }

            foreach (var triggerItem in triggerItems)
            {
                Debug.WriteLine($"[Aggregated] Application={triggerItem.Application.Properties.ApplicationName}; Service={triggerItem.Service.Properties.ServiceName}");
                foreach (var metric in triggerItem.Metrics)
                {
                    Debug.WriteLine($"[Aggregated] Name={metric.Name}; Value={metric.Value}");
                }
            }
            return triggerItems;
        }
    }
}
