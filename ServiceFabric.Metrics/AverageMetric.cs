namespace ServiceFabric.Metrics
{
    public class AverageMetric : MetricBase
    {
        private int _sum;
        private int _numValues;

        public AverageMetric(string name)
            : base(name)
        {
        }

        public override MetricType Type => MetricType.Average;
        public override int? Value => (_numValues > 0 ? (int?)((double)_sum / (double)_numValues) : null);

        public override void Reset()
        {
            _sum = 0;
            _numValues = 0;
        }

        public void AddValue(int value)
        {
            _sum += value;
            _numValues++;
        }
    }
}
