using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Watchdog;
using ServiceFabric.Watchdog.RuleEngine.Rules;
using ServiceFabric.Watchdog.RuleEngine.Actions;

namespace WatchdogStatefulSampleService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class WatchdogStatefulSampleService : StatefulService
    {
        public WatchdogStatefulSampleService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // create the watchdog and create our sample rule
            var ruleWatchdog = new RuleWatchdog(StateManager);
            await ruleWatchdog.Load();

            // NOTE: Since this is a stateful service, it will reload rules from reliable storage
            // this code only has to run once. Keeping it here for this example though
            await ruleWatchdog.AddRule(new Rule()
            {
                Name = "IterationCount",
                // only applicable for applications with Watchdog in their name
                RuleFilter = "Application == \'*Watchdog*\'",
                // action will trigger when Iterations are above 45
                TriggerExpression = "Iterations > 45",
                // We should aggretage data for the instances of the service
                AggregateData = true,
                // The expression need to trigger for 1 minute before action kicks in
                TriggerPeriod = new TimeSpan(0, 1, 0),
                // 2 minutes need to expire before the expression is considered again after an action has been done
                ActionGracePeriod = new TimeSpan(0, 2, 0),
                // we want to scale the service up, 1 at the time until we have the service on all nodes in the cluster
                TriggerAction = new ScaleStatelessServiceUpRuleAction()
                {
                    MaxNumInstances = -1,
                    MinNumInstances = 1,
                    ScaleDeltaNumInstances = 1
                }
            });
            // END NOTE

            // start the watchdog and check the rules every 10 seconds
            ruleWatchdog.Start(new TimeSpan(0, 0, 10));

            try
            {
                while (true)
                {
                    // just to keep the loop running
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
            catch (Exception)
            {
                ruleWatchdog.Stop();
            }
        }
    }
}
