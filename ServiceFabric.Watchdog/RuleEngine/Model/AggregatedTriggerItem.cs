using ServiceFabric.Watchdog.QueryObjects;
using System.Collections.Generic;

namespace ServiceFabric.Watchdog.RuleEngine.Model
{
    public class AggregatedTriggerItem
    {
        public SFApplication Application { get; set; }
        public SFService Service { get; set; }
        public SFPartition Partition { get; set; }
        public SFReplica Replica { get; set; }
        public Dictionary<string, AggregatedTriggerItemValue> TriggerItems { get; } = new Dictionary<string, AggregatedTriggerItemValue>();
    }
}
