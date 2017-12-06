using ServiceFabric.Watchdog.RuleEngine.Model;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public class StringExpressionValueService : StringExpressionValue
    {
        public override string GetValue(TriggerItem triggerItem)
        {
            return (triggerItem != null ? triggerItem.Service.Properties.ServiceName.AbsolutePath : null);
        }
    }
}
