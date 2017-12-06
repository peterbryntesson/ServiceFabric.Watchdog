using ServiceFabric.Watchdog.RuleEngine.Model;
using System;
using System.Linq;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public enum StringExpressionParserState
    {
        BeforeValue1,
        Value1,
        Operator,
        BeforeValue2,
        Value2,
        Complete,
        Closed,
        Error
    };

    public class StringExpression : Expression<string>
    {
        private static char[] _stringSeparators = { '=', '!', '|', '&', '(', ')', '\'', '\"', ' ' };
        private static char[] _stringOperators = { '=', '!', '|', '&' };

        public StringExpression(string expression = null)
            : base(_stringSeparators, _stringOperators, expression)
        {
        }

        protected override Expression<string> CreateExpression()
        {
            return new StringExpression();
        }

        protected override ExpressionValue<string> CreateLeftExpressionValue(string value)
        {
            if (value.ToLower() == "application")
                return new StringExpressionValueApplication();
            else if (value.ToLower() == "applicationtype")
                return new StringExpressionValueApplicationType();
            else if (value.ToLower() == "service")
                return new StringExpressionValueService();
            else if (value.ToLower() == "servicetype")
                return new StringExpressionValueServiceType();
            else
                return null;
        }

        protected override ExpressionValue<string> CreateRightExpressionValue(string value)
        {
            return new StringExpressionValueStatic(value);
        }

        public override string Execute(TriggerItem triggerItem)
        {
            string leftValue = LeftValue.GetValue(triggerItem);
            string rightValue = RightValue.GetValue(triggerItem);

            switch (Operator)
            {
                case ExpressionOperator.Equal:
                case ExpressionOperator.NotEqual:
                    bool result = false;
                    if (rightValue.StartsWith("*") && rightValue.EndsWith("*"))
                        result = leftValue.Contains(rightValue.Substring(1, rightValue.Length - 2));
                    else if (rightValue.StartsWith("*"))
                        result = leftValue.StartsWith(rightValue.Substring(1));
                    else if (rightValue.EndsWith("*"))
                        result = leftValue.StartsWith(rightValue.Substring(0, rightValue.Length - 1));
                    else
                        result = leftValue == rightValue;
                    result = (Operator == ExpressionOperator.Equal ? result : !result);
                    return (result ? "1" : "0");
                case ExpressionOperator.Contains:
                    return (leftValue.Contains(rightValue) ? "1" : "0");
                case ExpressionOperator.StartsWith:
                    return (leftValue.StartsWith(rightValue) ? "1" : "0");
                case ExpressionOperator.EndsWith:
                    return (leftValue.EndsWith(rightValue) ? "1" : "0");
                case ExpressionOperator.And:
                    return (leftValue == "1" && rightValue == "1" ? "1" : "0");
                case ExpressionOperator.Or:
                    return (leftValue == "1" || rightValue == "1" ? "1" : "0");
                default:
                    return "0";
            }
        }

    }
}
