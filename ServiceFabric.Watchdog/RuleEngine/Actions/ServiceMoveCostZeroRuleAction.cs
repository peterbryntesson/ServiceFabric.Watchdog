using ServiceFabric.Watchdog.RuleEngine.Model;
using System.Fabric;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    [DataContract]
    public class ServiceMoveCostZeroRuleAction : ServiceMoveCostRuleAction
    {
        public override Task Execute(TriggerItem triggerItem)
        {
            TargetMoveCost = MoveCost.Zero;
            return base.Execute(triggerItem);
        }
    }
}
