using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFabric.Metrics
{
    public class TimePercentageMetric : MetricBase
    {
        private TimeSpan _totalTime;
        private TimeSpan _elapsedTime;
        private TimeSpan _lastTotalElapsedTime;
        private DateTime _timeStarted;
        private int _divider;

        public TimePercentageMetric(string name, int divider = 1)
            : base(name)
        {
            if (divider == 0)
                divider = 1;
            _divider = divider;
        }

        public override MetricType Type => MetricType.TimePercentage;
        public override int? Value => (_totalTime.TotalMilliseconds > 0 ? (int?)((_elapsedTime.TotalMilliseconds / _totalTime.TotalMilliseconds) * 100 / _divider) : null);

        public override void Reset()
        {
            _totalTime = default(TimeSpan);
            _elapsedTime = default(TimeSpan);
            _timeStarted = DateTime.UtcNow;
        }

        public void SetValue(TimeSpan value)
        {
            TimeSpan totalElapsedTime = value;
            _totalTime = DateTime.UtcNow - _timeStarted;
            _elapsedTime += totalElapsedTime - _lastTotalElapsedTime;
            _lastTotalElapsedTime = totalElapsedTime;
        }

        public void SetTotalElapsedTime(TimeSpan value)
        {
            _lastTotalElapsedTime = value;
        }
    }
}
