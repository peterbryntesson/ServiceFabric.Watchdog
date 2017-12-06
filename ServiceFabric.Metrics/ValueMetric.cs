using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFabric.Metrics
{
    public class ValueMetric : MetricBase
    {
        private int? _value;

        public ValueMetric(string name)
            : base(name)
        {

        }

        public override MetricType Type => MetricType.Value;
        public override int? Value => _value;

        public override void Reset()
        {
            _value = null;
        }

        public void SetValue(int value)
        {
            _value = value;
        }
    }
}
