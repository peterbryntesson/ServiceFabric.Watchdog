using ServiceFabric.Watchdog.RuleEngine.Model;
using System.Fabric;
using System.Fabric.Description;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    public class ServiceMoveCostRuleAction : RuleAction
    {
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
