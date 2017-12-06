using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public enum ExpressionParserState
    {
        BeforeLeftValue,
        LeftValue,
        Operator,
        BeforeRightValue,
        RightValue,
        Complete,
        Closed,
        Error
    }
}
