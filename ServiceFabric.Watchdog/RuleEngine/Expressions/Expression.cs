using ServiceFabric.Watchdog.RuleEngine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.RuleEngine.Expressions
{
    public abstract class Expression<TValue>
    {
        protected char[] _separators;
        protected char[] _operators;

        public ExpressionValue<TValue> LeftValue { get; set; }
        public ExpressionValue<TValue> RightValue { get; set; }
        public ExpressionOperator Operator { get; set; } = ExpressionOperator.Unknown;

        public Expression(char[] separators, char[] operators, string expression = null)
        {
            _separators = separators;
            _operators = operators;
            if (!String.IsNullOrEmpty(expression))
                Parse(expression);
        }

        public bool Parse(string expression)
        {
            LeftValue = null;
            Operator = ExpressionOperator.Unknown;
            RightValue = null;
            int startIndex = 0;
            var parsedExpression = ParseSubExpression(expression, 0, startIndex, out int stopIndex);
            if (parsedExpression != null)
            {
                LeftValue = parsedExpression.LeftValue;
                Operator = parsedExpression.Operator;
                RightValue = parsedExpression.RightValue;
                return true;
            }
            else
                return false;
        }

        public abstract TValue Execute(TriggerItem triggerItem);
        protected abstract Expression<TValue> CreateExpression();
        protected abstract ExpressionValue<TValue> CreateLeftExpressionValue(string value);
        protected abstract ExpressionValue<TValue> CreateRightExpressionValue(string value);

        protected Expression<TValue> ParseSubExpression(string expression, int level, int startIndex, out int stopIndex)
        {
            var parsedExpression = CreateExpression();
            ExpressionParserState state = ExpressionParserState.BeforeLeftValue;
            bool hyphened = false;
            int index = startIndex;

            // loop until we are done
            while (index < expression.Length && state != ExpressionParserState.Closed && state != ExpressionParserState.Error)
            {
                switch (state)
                {
                    case ExpressionParserState.BeforeLeftValue:
                    case ExpressionParserState.BeforeRightValue:
                        // okay, what the first character?
                        if (expression[index] == '(')
                        {
                            // increase level and parse sub expression
                            var subExpression = ParseSubExpression(expression, level + 1, index + 1, out stopIndex);
                            if (subExpression != null)
                            {
                                if (state == ExpressionParserState.BeforeLeftValue)
                                    parsedExpression.LeftValue = new ExpressionValueExpression<TValue>(subExpression);
                                else
                                    parsedExpression.RightValue = new ExpressionValueExpression<TValue>(subExpression);
                                state = (state == ExpressionParserState.BeforeLeftValue ? ExpressionParserState.Operator : ExpressionParserState.Complete);
                                index = stopIndex;
                                if (index < expression.Length && expression[index] == ')')
                                    index++;
                                while (index < expression.Length && expression[index] == ' ')
                                    index++;
                            }
                            else
                            {
                                // ignore parenthis and move on
                                index++;
                            }
                            break;
                        }
                        else if (expression[index] == '\'' || expression[index] == '\"')
                        {
                            // move on pass this and move to state value1
                            index++;
                            hyphened = true;
                            state = (state == ExpressionParserState.BeforeLeftValue ? ExpressionParserState.LeftValue : ExpressionParserState.RightValue);
                        }
                        else
                        {
                            // move to state value1
                            state = (state == ExpressionParserState.BeforeLeftValue ? ExpressionParserState.LeftValue : ExpressionParserState.RightValue);
                        }
                        break;

                    case ExpressionParserState.LeftValue:
                    case ExpressionParserState.RightValue:
                        int startOperand = index;
                        // read out the value1 operand
                        while (index < expression.Length &&
                               (hyphened ? (expression[index] != '\'' && expression[index] != '\"') :
                               !_separators.Contains(expression[index])))
                            index++;
                        if (state == ExpressionParserState.LeftValue)
                        {
                            parsedExpression.LeftValue = CreateLeftExpressionValue(expression.Substring(startOperand, index - startOperand));
                            if (parsedExpression.LeftValue == null)
                                state = ExpressionParserState.Error;
                        }
                        else
                        {
                            parsedExpression.RightValue = CreateRightExpressionValue(expression.Substring(startOperand, index - startOperand));
                            if (parsedExpression.LeftValue == null)
                                state = ExpressionParserState.Error;
                        }
                        while (index < expression.Length && (expression[index] == '\'' || expression[index] == '\"' || expression[index] == ' '))
                            index++;
                        // move to next state
                        if (state != ExpressionParserState.Error)
                            state = (state == ExpressionParserState.LeftValue ? ExpressionParserState.Operator : ExpressionParserState.Complete);
                        hyphened = false;
                        break;

                    case ExpressionParserState.Operator:
                        parsedExpression.Operator = OperatorFromString(expression.Substring(index));
                        if (parsedExpression.Operator == ExpressionOperator.Unknown)
                        {
                            state = ExpressionParserState.Error;
                        }
                        else
                        {
                            state = ExpressionParserState.BeforeRightValue;
                            while (index < expression.Length && (_operators.Contains(expression[index]) || expression[index] == ' '))
                                index++;
                        }
                        break;

                    case ExpressionParserState.Complete:
                        if (index >= expression.Length || (level > 0 && expression[index] == ')'))
                            state = ExpressionParserState.Closed;
                        else if (_operators.Contains(expression[index]))
                        {
                            var op = OperatorFromString(expression.Substring(index));
                            if (op != ExpressionOperator.And && op != ExpressionOperator.Or)
                            {
                                state = ExpressionParserState.Error;
                                break;
                            }

                            while (index < expression.Length && (_operators.Contains(expression[index]) || expression[index] == ' '))
                                index++;

                            var newExpression = CreateExpression();
                            newExpression.LeftValue = new ExpressionValueExpression<TValue>(parsedExpression);
                            newExpression.Operator = op;
                            parsedExpression = newExpression;
                            if (expression[index] == '(')
                                index++;
                            var subExpression = ParseSubExpression(expression, level + 1, index, out stopIndex);
                            if (subExpression != null)
                            {
                                parsedExpression.RightValue = new ExpressionValueExpression<TValue>(subExpression);
                                state = ExpressionParserState.Complete;
                            }
                            else
                                state = ExpressionParserState.Error;
                            index = stopIndex;
                            while (index < expression.Length && expression[index] == ' ')
                                index++;
                        }
                        else
                            state = ExpressionParserState.Error;
                        break;
                }
            }

            stopIndex = index;
            return (parsedExpression.LeftValue != null && parsedExpression.Operator != ExpressionOperator.Unknown && parsedExpression.RightValue != null ?
                        parsedExpression : null);
        }

        public string ToString(TriggerItem triggerItem)
        {
            return $"{LeftValue.ToString(triggerItem)} {OperatorAsString(Operator)} {RightValue.ToString(triggerItem)}";
        }

        private string OperatorAsString(ExpressionOperator op)
        {
            switch (op)
            {
                case ExpressionOperator.Add: return "+";
                case ExpressionOperator.And: return "&&";
                case ExpressionOperator.Divide: return "/";
                case ExpressionOperator.Equal: return "==";
                case ExpressionOperator.Larger: return ">";
                case ExpressionOperator.LargerOrEqual: return ">=";
                case ExpressionOperator.Multiply: return "*";
                case ExpressionOperator.NotEqual: return "!=";
                case ExpressionOperator.Or: return "||";
                case ExpressionOperator.Smaller: return "<";
                case ExpressionOperator.SmallerOrEqual: return "<=";
                case ExpressionOperator.Subtract: return "-";
                case ExpressionOperator.Contains: return "contains";
                case ExpressionOperator.StartsWith: return "startswith";
                case ExpressionOperator.EndsWith: return "endswith";
                default: return "--";
            }
        }

        private ExpressionOperator OperatorFromString(string op)
        {
            if (op.Substring(0, 1) == "+")
                return ExpressionOperator.Add;
            else if (op.Substring(0, 2) == "&&")
                return ExpressionOperator.And;
            else if (op.Substring(0, 1) == "/")
                return ExpressionOperator.Divide;
            else if (op.Substring(0, 2) == "==")
                return ExpressionOperator.Equal;
            else if (op.Substring(0, 1) == "=")
                return ExpressionOperator.Equal;
            else if (op.Substring(0, 2) == ">=")
                return ExpressionOperator.LargerOrEqual;
            else if (op.Substring(0, 1) == ">")
                return ExpressionOperator.Larger;
            else if (op.Substring(0, 1) == "*")
                return ExpressionOperator.Multiply;
            else if (op.Substring(0, 2) == "!=")
                return ExpressionOperator.NotEqual;
            else if (op.Substring(0, 2) == "||")
                return ExpressionOperator.Or;
            else if (op.Substring(0, 2) == "<=")
                return ExpressionOperator.SmallerOrEqual;
            else if (op.Substring(0, 1) == "<")
                return ExpressionOperator.Smaller;
            else if (op.Substring(0, 1) == "-")
                return ExpressionOperator.Subtract;
            else
                return ExpressionOperator.Unknown;
        }
    }
}
