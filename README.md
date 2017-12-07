# ServiceFabric.Watchdog
ServiceFabric.Watchdog is a little framework that helps you emit custom metrics from your Azure Service Fabric Service and monitor/act on those metrics in a watchdog. An example of what you can do is to expose metrics like *AverageResponseTime* and *RequestsPerSecond*, and based on those metrics you can setup rules that scales up or down the number of instances of that service. Interesting? Here are the details!
## Emitting metrics
The first thing you have to do is emit custom metrics. In the **ServiceFabric.Watchdog** solution you will find two projects:
* **ServiceFabric.Metrics**, which is a .NET Standard 2.0 library that contains various base classes that implements different types of metrics, like:
  * Value
  * Sum
  * Average
  * High
  * Low

  You derive your own metrics from one of these base classes, and either submit the values yourself as they happen via *SetValue*/*AddValue* or you override *ProvideValue* and call *SetValue*/*AddValue* there. This needs to be done for all your metrics that you want to expose. Here is an example from the sample:
    ```csharp
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
    ```

* **ServiceFabric.Watchdog** is a .NET Framework that contains the rest of the implementation. Of interest is the *MetricsBuilder* class. Create an instance of it, add your metrics and call it's *Start* method. In the sample we do this in *RunASync* like this:

    ```csharp
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
    ```

Now, in addition to this we need to specify load metrics when we create the service. You can do this in many ways depending on how you create the service - I do it through the [Service Fabric Explorer UI](http://localhost:19080). On the *MetricsBuilderSample* Service Type, press **Create** and specify the metrics like this:

![Create Service with load metrics](https://github.com/peterbryntesson/ServiceFabric.Watchdog/master/blob/CreateService,png "Create Service with load metrics")

## Creating the watchdog
Ok, we emit the metrics we want. Now we want to monitor them and take actions on them as well. For that we use the *RuleWatchdog* class and add rules to it. The properties in the *Rule* class are:
* **RuleFilter**. Here you can filter on which objects you want to apply the rule on. You can filter on:
  * Application
  * ApplicationType
  * Service
  * ServiceType

  You can filter on exact match, starts with, ends with and contains. You can combine filters with *and* or *or*, for example like this: "Application = '*Watchdog' && Service = 'MetricsBuilderSample'". If you leave this empty no service is ruled out.
* **TriggerExpression**. This is the expression that uses the metrics emitted. In our sample we have a fairly simple version, but you can build as complex patterns as you like. In the sample we use: "Iterations > 45"
* **AggregateData**. This property defines if we should aggregate the metrics across all running instances or not.  
* **TriggerAction**. This is the action we want to take if the **TriggerExpression** evaluates to true. This could be anything; right now we support the following actions (but more can be added by deriving from the **Action** base class):
  * Scaling stateless services up or down.
  * Changing the *MoveCost* property for a service.
  * Deleting the running service instance.
* **TriggerPeriod**. To avoid suddens spikes in the metrics causing undesired actions, you can specify a time interval here. This interval defines the time the **TriggerExpression** *continously* triggers before the action is taken.
* **ActionGracePeriod**. To stop actions firing rapidly you can assign a time interval that hinders an action to fire again. This interval will then stop this action to fire again before it has expired.

Putting it together, here is the code in **WatchdogSampleService** that set up the rule and starts the *RuleWatchdog* instance:

    ```csharp
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
            ActionGracePeriod = new TimeSpan(0, 2, 0),
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
    ```

That's it! Test out the sample, implement it in your own solution and improve it by doing a pull request!



