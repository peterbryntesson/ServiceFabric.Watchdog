using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Watchdog;
using ServiceFabric.Metrics;

namespace MetricsBuilderSampleService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class MetricsBuilderSampleService : StatelessService
    {
        private long _iterations = 0;

        public MetricsBuilderSampleService(StatelessServiceContext context)
            : base(context)
        { }

        public long Iterations { get { return _iterations; } }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // setup the metrics and start the metrics builder on a 5 second interval
            var metricsBuilder = new MetricsBuilder();
            metricsBuilder.Add(new CPUMetric());
            metricsBuilder.Add(new WorkingSetMetric());
            metricsBuilder.Add(new IterationsCountMetric(this));
            metricsBuilder.Start(this.Partition, new TimeSpan(0, 0, 5));

            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++_iterations);

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
            catch(Exception)
            {
                metricsBuilder.Stop();
            }
        }
    }
}
