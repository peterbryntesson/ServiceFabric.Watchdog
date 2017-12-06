using ServiceFabric.Watchdog.RuleEngine.Model;
using System.Fabric;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    public abstract class RuleAction
    {
        protected FabricClient _fabricClient = new FabricClient();

        public abstract Task Execute(TriggerItem triggerItem);
    }
}
