using ServiceFabric.Watchdog.RuleEngine.Model;
using System.Fabric;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    public class ServiceMoveCostZeroRuleAction : ServiceMoveCostRuleAction
    {
        public override Task Execute(TriggerItem triggerItem)
        {
            TargetMoveCost = MoveCost.Zero;
            return base.Execute(triggerItem);
        }
    }
}
