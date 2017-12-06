using ServiceFabric.Watchdog.QueryObjects;
using System.Collections.Generic;
using System.Text;

namespace ServiceFabric.Watchdog.RuleEngine.Model
{
    public class TriggerItem
    {
        public SFApplication Application { get; set; }
        public SFService Service { get; set; }
        public SFPartition Partition { get; set; }
        public SFReplica Replica { get; set; }
        public List<TriggerItemMetric> Metrics { get; } = new List<TriggerItemMetric>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Application={Application.Properties.ApplicationName}; Service={Service.Properties.ServiceName}; ");
            sb.Append($"Partition={Partition.Properties.PartitionInformation.Id}; Replica={Replica.Properties.Id}; ");
            sb.Append("has following metrics: [");
            foreach (var metric in Metrics)
            {
                sb.Append($"[Name={metric.Name}; Value={metric.Value}]");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
