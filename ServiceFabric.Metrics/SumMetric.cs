using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFabric.Metrics
{
    public class SumMetric : MetricBase
    {
        private int? _sum;

        public SumMetric(string name)
            : base(name)
        {

        }
        public override MetricType Type => MetricType.Sum;
        public override int? Value => _sum;

        public override void Reset()
        {
            _sum = null;
        }

        public void AddValue(int value)
        {
            if (_sum.HasValue)
                _sum += value;
            else
                _sum = value;
        }
    }
}
