using ServiceFabric.Watchdog.RuleEngine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public class StringExpressionValueStatic : StringExpressionValue
    {
        private string _value;

        public StringExpressionValueStatic(string value)
        {
            _value = value;
        }

        public override string GetValue(TriggerItem triggerItem)
        {
            return _value;
        }
    }
}
