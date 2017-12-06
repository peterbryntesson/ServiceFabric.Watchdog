using ServiceFabric.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog
{
    public class MetricsBuilder
    {
        protected IServicePartition _partition;
        protected TimeSpan _reportInterval;
        protected DateTime _timeIntervalStarted;
        public List<MetricBase> _metrics = new List<MetricBase>();
        protected System.Timers.Timer _timer = new System.Timers.Timer();

        public void Add(MetricBase metric)
        {
            _metrics.Add(metric);
        }

        public void Start(IServicePartition partition, TimeSpan reportLoadInterval)
        {
            _partition = partition;
            _reportInterval = reportLoadInterval;

            foreach (var metric in _metrics)
                metric.Start();

            _timeIntervalStarted = DateTime.UtcNow;
            _timer.Interval = Math.Min(1000, _reportInterval.TotalMilliseconds / 2);
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            foreach (var metric in _metrics)
                metric.Reset();
        }

        private async void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
            foreach (var metric in _metrics)
            {
                try
                {
                    await metric.ProvideValue();
                }
                catch (Exception)
                {
                }
            }

            if ((DateTime.UtcNow - _timeIntervalStarted) > _reportInterval)
            {
                ReportLoad();
                foreach (var metric in _metrics)
                    metric.Reset();
                _timeIntervalStarted = DateTime.UtcNow;
            }
            _timer.Start();
        }

        private void ReportLoad()
        {
            var loadMetrics = new List<LoadMetric>();
            foreach (var metric in _metrics)
                if (metric.Value.HasValue)
                    loadMetrics.Add(new LoadMetric(metric.Name, metric.Value.Value));

#if DEBUG
            foreach (var loadMetric in loadMetrics)
            {
                Debug.WriteLine($"[{loadMetric.Name}, {loadMetric.Value}]");
            }
#endif
            _partition.ReportLoad(loadMetrics);
        }
    }
}
