using ServiceFabric.Watchdog.RuleEngine.Model;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public class IntExpressionValueStatic : IntExpressionValue
    {
        private int _value;

        public IntExpressionValueStatic(int value)
        {
            _value = value;
        }

        public override int GetValue(TriggerItem triggerItem)
        {
            return _value;
        }

        public override string ToString(TriggerItem triggerItem)
        {
            return _value.ToString();
        }
    }
}
