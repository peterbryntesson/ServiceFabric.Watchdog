using ServiceFabric.Watchdog.RuleEngine.Model;
using System.Fabric.Description;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    [DataContract]
    public abstract class ScaleStatelessServiceRuleAction : RuleAction
    {
        [DataMember]
        public int MinNumInstances { get; set; }
        [DataMember]
        public int MaxNumInstances { get; set; }
        [DataMember]
        public int ScaleDeltaNumInstances { get; set; }
        [DataMember]
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
