using System;

namespace ServiceFabric.Metrics
{
    public class ValuePerSecMetric : MetricBase
    {
        private TimeSpan _totalTime;
        private DateTime _timeStarted;
        private int? _sum;

        public ValuePerSecMetric(string name)
            : base(name)
        {
        }

        public override MetricType Type => MetricType.ValuePerSec;
        public override int? Value => (_totalTime.TotalSeconds > 0 && _sum.HasValue ? (int?)(_sum.Value / _totalTime.TotalSeconds) : null);

        public override void Reset()
        {
            _totalTime = default(TimeSpan);
            _timeStarted = DateTime.UtcNow;
            _sum = null;
        }

        public void AddValue(int value)
        {
            _sum = (_sum.HasValue ? _sum.Value + value : value);
            _totalTime = DateTime.UtcNow - _timeStarted;
        }
    }
}
