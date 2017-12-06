using ServiceFabric.Watchdog.RuleEngine.Model;
using System.Linq;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public class IntExpressionValueDynamic : IntExpressionValue
    {
        private string _metricName;

        public IntExpressionValueDynamic(string metricName)
        {
            _metricName = metricName;
        }

        public override int GetValue(TriggerItem triggerItem)
        {
            var loadMetric = triggerItem.Metrics.Where(r => r.Name == _metricName).FirstOrDefault();
            return (loadMetric != null ? loadMetric.Value : 0);
        }

        public override string ToString(TriggerItem triggerItem)
        {
            var loadMetric = triggerItem.Metrics.Where(r => r.Name == _metricName).FirstOrDefault();
            return (loadMetric != null ? loadMetric.Value.ToString() : "<null>");
        }
    }
}
