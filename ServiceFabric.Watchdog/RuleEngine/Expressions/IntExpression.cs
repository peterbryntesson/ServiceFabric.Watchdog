using ServiceFabric.Watchdog.RuleEngine.Model;
using System;
using System.Linq;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public class IntExpression : Expression<int>
    {
        private static char[] _intSeparators = { '+', '-', '*', '/', '=', '!', '>', '<', '|', '&', '(', ')', '\'', '\"', ' ' };
        private static char[] _intOperators = { '+', '-', '*', '/', '=', '!', '>', '<', '|', '&' };

        public IntExpression(string expression = null) 
            : base (_intSeparators, _intOperators, expression)
        {
        }

        protected override Expression<int> CreateExpression()
        {
            return new IntExpression();
        }

        protected override ExpressionValue<int> CreateLeftExpressionValue(string value)
        {
            return new IntExpressionValueDynamic(value);
        }

        protected override ExpressionValue<int> CreateRightExpressionValue(string value)
        {
            if (Int32.TryParse(value, out int intValue))
                return new IntExpressionValueStatic(intValue);
            else
                return null;
        }

        public override int Execute(TriggerItem triggerItem)
        {
            int leftValue = LeftValue.GetValue(triggerItem);
            int rightValue = RightValue.GetValue(triggerItem);

            switch (Operator)
            {
                case ExpressionOperator.Add:
                    return leftValue + rightValue;
                case ExpressionOperator.Subtract:
                    return leftValue - rightValue;
                case ExpressionOperator.Multiply:
                    return leftValue * rightValue;
                case ExpressionOperator.Divide:
                    return leftValue / rightValue;
                case ExpressionOperator.Equal:
                    return (leftValue == rightValue ? 1 : 0);
                case ExpressionOperator.NotEqual:
                    return (leftValue != rightValue ? 1 : 0);
                case ExpressionOperator.Larger:
                    return (leftValue > rightValue ? 1 : 0);
                case ExpressionOperator.LargerOrEqual:
                    return (leftValue >= rightValue ? 1 : 0);
                case ExpressionOperator.Smaller:
                    return (leftValue < rightValue ? 1 : 0);
                case ExpressionOperator.SmallerOrEqual:
                    return (leftValue <= rightValue ? 1 : 0);
                case ExpressionOperator.And:
                    return ((leftValue != 0) && (rightValue != 0) ? 1 : 0);
                case ExpressionOperator.Or:
                    return ((leftValue != 0) || (rightValue != 0) ? 1 : 0);
                default:
                    return 0;
            }
        }

    }
}
