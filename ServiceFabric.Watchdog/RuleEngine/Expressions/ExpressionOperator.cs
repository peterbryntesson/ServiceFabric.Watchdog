using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public enum ExpressionOperator
    {
        Unknown,
        Add,
        Subtract,
        Multiply,
        Divide,
        Equal,
        NotEqual,
        Larger,
        LargerOrEqual,
        Smaller,
        SmallerOrEqual,
        Contains,
        StartsWith,
        EndsWith,
        And,
        Or
    }
}
