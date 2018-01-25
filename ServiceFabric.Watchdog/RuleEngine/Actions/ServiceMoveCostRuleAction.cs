using ServiceFabric.Watchdog.RuleEngine.Model;
using System.Fabric;
using System.Fabric.Description;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    [DataContract]
    public class ServiceMoveCostRuleAction : RuleAction
    {
        [DataMember]
        public MoveCost TargetMoveCost { get; set; }

        public override async Task Execute(TriggerItem triggerItem)
        {
            var serviceDescription = new StatelessServiceUpdateDescription()
            {
                DefaultMoveCost = TargetMoveCost
            };

            await _fabricClient.ServiceManager.UpdateServiceAsync(triggerItem.Service.Properties.ServiceName, serviceDescription);
        }
    }
}
