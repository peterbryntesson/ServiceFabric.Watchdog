using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFabric.Metrics
{
    public class HighValueMetric : MetricBase
    {
        private int? _highValue;

        public HighValueMetric(string name)
            : base(name)
        {

        }

        public override MetricType Type => MetricType.High;
        public override int? Value => _highValue;

        public override void Reset()
        {
            _highValue = null;
        }

        public void SetValue(int value)
        {
            if (_highValue.HasValue)
            {
                if (_highValue.Value < value)
                    _highValue += value;
            }
            else
                _highValue = value;
        }
    }
}
