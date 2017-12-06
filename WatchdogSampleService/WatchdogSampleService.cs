using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Watchdog;
using ServiceFabric.Watchdog.RuleEngine.Rules;
using ServiceFabric.Watchdog.RuleEngine.Expressions;
using ServiceFabric.Watchdog.RuleEngine.Actions;

namespace WatchdogSampleService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WatchdogSampleService : StatelessService
    {
        public WatchdogSampleService(StatelessServiceContext context)
            : base(context)
        { }

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
            // create the watchdog and create our sample rule
            var ruleWatchdog = new RuleWatchdog();
            ruleWatchdog.Rules.Add(new Rule()
            {
                // only applicable for applications with Watchdog in their name
                RuleFilter = new StringExpression("Application == \'*Watchdog*\'"),
                // action will trigger when Iterations are above 45
                TriggerExpression = new IntExpression("Iterations > 45"),
                // We should aggretage data for the instances of the service
                AggregateData = true,
                // The expression need to trigger for 1 minute before action kicks in
                TriggerPeriod = new TimeSpan(0, 1, 0),
                // 2 minutes need to expire before the expression is considered again after an action has been done
                TriggerGracePeriod = new TimeSpan(0, 2, 0),
                // we want to scale the service up, 1 at the time until we have the service on all nodes in the cluster
                TriggerAction = new ScaleStatelessServiceUpRuleAction()
                {
                    MaxNumInstances = -1,
                    MinNumInstances = 1,
                    ScaleDeltaNumInstances = 1
                }
            });
            // start the watchdog and check the rules every 10 seconds
            ruleWatchdog.Start(new TimeSpan(0, 0, 10));

            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // now, how to work with this metrics.
                    await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
                }
            }
            catch (Exception)
            {
                ruleWatchdog.Stop();
            }
        }
    }
}
