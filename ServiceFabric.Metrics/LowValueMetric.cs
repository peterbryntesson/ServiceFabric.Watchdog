namespace ServiceFabric.Metrics
{
    public class LowValueMetric : MetricBase
    {
        private int? _lowValue;

        public LowValueMetric(string name)
            : base(name)
        {

        }

        public override MetricType Type => MetricType.Low;
        public override int? Value => _lowValue;

        public override void Reset()
        {
            _lowValue = null;
        }

        public void SetValue(int value)
        {
            if (_lowValue.HasValue)
            {
                if (_lowValue.Value > value)
                    _lowValue += value;
            }
            else
                _lowValue = value;
        }
    }
}
