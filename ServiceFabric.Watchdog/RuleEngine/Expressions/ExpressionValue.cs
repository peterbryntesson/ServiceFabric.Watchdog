using ServiceFabric.Watchdog.RuleEngine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public abstract class ExpressionValue<TValue>
    {
        public abstract TValue GetValue(TriggerItem triggerItem);
        public abstract string ToString(TriggerItem triggerItem);
    }
}
