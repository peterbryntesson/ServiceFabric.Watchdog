using System;

namespace ServiceFabric.Watchdog.RuleEngine.Model
{
    public class TriggerItemMetric
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public DateTime LastReportedUtc { get; set; }
    }
}
