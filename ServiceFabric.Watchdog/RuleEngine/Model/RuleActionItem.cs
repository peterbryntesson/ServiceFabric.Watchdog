using ServiceFabric.Watchdog.RuleEngine.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Model
{
    public class RuleActionItem
    {
        public DateTime TimeTriggeredUtc { get; set; }
        public TriggerItem TriggerItem { get; set; }
        public RuleAction TriggerAction { get; set; }
    }
}
