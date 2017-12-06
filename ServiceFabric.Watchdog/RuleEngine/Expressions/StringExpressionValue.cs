using ServiceFabric.Watchdog.RuleEngine.Model;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public abstract class StringExpressionValue : ExpressionValue<string>
    {
        public override string ToString(TriggerItem triggerItem)
        {
            return GetValue(triggerItem);
        }
    }
}
