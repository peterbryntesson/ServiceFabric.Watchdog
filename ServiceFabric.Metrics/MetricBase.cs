using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Metrics
{
    public abstract class MetricBase
    {
        public MetricBase(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
        public abstract MetricType Type { get; }
        public virtual int? Value { get { return 0; } }

        public virtual void Reset()
        {
        }

        public virtual void Start()
        {
            // base class only calls reset
            Reset();
        }

        public virtual Task ProvideValue()
        {
            // base class does nothing - override if needed
            return Task.CompletedTask;
        }
    }
}
