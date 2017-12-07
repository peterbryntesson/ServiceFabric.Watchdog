using ServiceFabric.Metrics;
using System.Threading.Tasks;

namespace MetricsBuilderSampleService
{
    internal class IterationsCountMetric : ValueMetric
    {
        private MetricsBuilderSampleService _service;
        public IterationsCountMetric(MetricsBuilderSampleService service)
            : base("Iterations")
        {
            _service = service;
        }

        public override Task ProvideValue()
        {
            SetValue((int)_service.Iterations);
            return Task.CompletedTask;
        }
    }
}
