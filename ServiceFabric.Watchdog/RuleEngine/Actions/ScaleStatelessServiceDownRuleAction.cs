using ServiceFabric.Watchdog.RuleEngine.Model;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    [DataContract]
    public class ScaleStatelessServiceDownRuleAction : ScaleStatelessServiceRuleAction
    {
        public override async Task Execute(TriggerItem triggerItem)
        {
            var serviceInstances = await _fabricClient.QueryManager.GetReplicaListAsync(triggerItem.Partition.Properties.PartitionInformation.Id);
            TargetNumInstances = serviceInstances.Count - ScaleDeltaNumInstances;
            var minNumInstances = MinNumInstances;
            if (minNumInstances == -1)
            {
                var nodes = await _fabricClient.QueryManager.GetNodeListAsync();
                minNumInstances = nodes.Count;
            }
            TargetNumInstances = Math.Max(TargetNumInstances, minNumInstances);
            await base.Execute(triggerItem);
        }
    }
}
