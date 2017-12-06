using ServiceFabric.Watchdog.RuleEngine.Model;
using System.Fabric.Description;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    public abstract class ScaleStatelessServiceRuleAction : RuleAction
    {
        public int MinNumInstances { get; set; }
        public int MaxNumInstances { get; set; }
        public int ScaleDeltaNumInstances { get; set; }
        public int TargetNumInstances { get; protected set; }

        public override async Task Execute(TriggerItem triggerItem)
        {
            var serviceDescription = new StatelessServiceUpdateDescription()
            {
                InstanceCount = TargetNumInstances
            };

            await _fabricClient.ServiceManager.UpdateServiceAsync(triggerItem.Service.Properties.ServiceName, serviceDescription);
        }
    }
}
