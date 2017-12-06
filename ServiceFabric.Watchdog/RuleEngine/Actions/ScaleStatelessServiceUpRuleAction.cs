using ServiceFabric.Watchdog.RuleEngine.Model;
using System;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    public class ScaleStatelessServiceUpRuleAction : ScaleStatelessServiceRuleAction
    {
        public override async Task Execute(TriggerItem triggerItem)
        {
            var serviceInstances = await _fabricClient.QueryManager.GetReplicaListAsync(triggerItem.Partition.Properties.PartitionInformation.Id);
            TargetNumInstances = serviceInstances.Count + ScaleDeltaNumInstances;
            var maxNumInstances = MaxNumInstances;
            if (maxNumInstances == -1)
            {
                var nodes = await _fabricClient.QueryManager.GetNodeListAsync();
                maxNumInstances = nodes.Count;
            }
            TargetNumInstances = Math.Min(TargetNumInstances, maxNumInstances);
            await base.Execute(triggerItem);
        }
    }
}
