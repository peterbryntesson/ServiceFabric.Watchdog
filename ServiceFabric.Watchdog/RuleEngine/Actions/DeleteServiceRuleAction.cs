using ServiceFabric.Watchdog.RuleEngine.Model;
using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Actions
{
    public class DeleteServiceRuleAction : RuleAction
    {
        public override async Task Execute(TriggerItem triggerItem)
        {
            var serviceDescription = new DeleteServiceDescription(triggerItem.Service.Properties.ServiceName) { ForceDelete = true };
            await _fabricClient.ServiceManager.DeleteServiceAsync(serviceDescription);
        }
    }
}
