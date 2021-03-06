﻿using System.Diagnostics;
using System.Threading.Tasks;

namespace ServiceFabric.Metrics
{
    public class WorkingSetMetric : ValueMetric
    {
        public WorkingSetMetric()
            : base("WorkingSet")
        {

        }

        public override Task ProvideValue()
        {
            using (var proc = Process.GetCurrentProcess())
                SetValue((int)proc.WorkingSet64);
            return Task.CompletedTask;
        }
    }
}
