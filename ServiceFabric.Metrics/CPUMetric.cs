using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Metrics
{
    public class CPUMetric : TimePercentageMetric
    {
        public CPUMetric()
            : base("CPU", Environment.ProcessorCount)
        {
        }

        public override void Start()
        {
            using (var proc = Process.GetCurrentProcess())
                SetTotalElapsedTime(proc.TotalProcessorTime);
            base.Start();
        }

        public override Task ProvideValue()
        {
            using (var proc = Process.GetCurrentProcess())
                SetValue(proc.TotalProcessorTime);
            return Task.CompletedTask;
        }
    }
}
