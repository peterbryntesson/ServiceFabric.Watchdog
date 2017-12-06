using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFabric.Metrics
{
    public enum MetricType
    {
        Value,
        Sum,
        Average,
        High,
        Low,
        ValuePerSec,
        TimePercentage
    };
}
