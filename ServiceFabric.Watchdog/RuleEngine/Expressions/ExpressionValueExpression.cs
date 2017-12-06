using ServiceFabric.Watchdog.RuleEngine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public class ExpressionValueExpression<TValue> : ExpressionValue<TValue>
    {
        private Expression<TValue> _expression;

        public ExpressionValueExpression(Expression<TValue> expression)
        {
            _expression = expression;
        }

        public override TValue GetValue(TriggerItem triggerItem)
        {
            return (TValue)_expression.Execute(triggerItem);
        }

        public override string ToString(TriggerItem triggerItem)
        {
            return $"({_expression.ToString(triggerItem)})";
        }
    }
}
