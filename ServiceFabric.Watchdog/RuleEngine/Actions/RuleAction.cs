using ServiceFabric.Watchdog.RuleEngine.Model;
using System.Fabric;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    [DataContract]
    public abstract class RuleAction
    {
        protected FabricClient _fabricClient = new FabricClient();

        public abstract Task Execute(TriggerItem triggerItem);
    }
}
