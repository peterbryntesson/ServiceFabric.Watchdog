using ServiceFabric.Watchdog.RuleEngine.Model;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public class StringExpressionValueApplication : StringExpressionValue
    {
        public override string GetValue(TriggerItem triggerItem)
        {
            return (triggerItem != null ? triggerItem.Application.Properties.ApplicationName.AbsolutePath : null);
        }
    }
}
