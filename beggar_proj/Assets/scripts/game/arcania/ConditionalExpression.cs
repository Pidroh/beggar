using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace arcania
{

    public enum ComparisonOperator
    {
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Equal,
        NotEqual,
        And,
        Or
    }


    public class ConditionalExpression
    {
        public string rawExpression;
        public string humanExpression;
        public ConditionalExpressionData expression;

        internal static bool Evaluate(float value1, int value2, ComparisonOperator op)
        {
            switch (op)
            {
                case ComparisonOperator.GreaterThan:
                    return value1 > value2;
                case ComparisonOperator.GreaterThanOrEqual:
                    return value1 >= value2;
                case ComparisonOperator.LessThan:
                    return value1 < value2;
                case ComparisonOperator.LessThanOrEqual:
                    return value1 <= value2;
                case ComparisonOperator.Equal:
                    return Math.Abs(value1 - value2) < float.Epsilon;
                case ComparisonOperator.NotEqual:
                    return Math.Abs(value1 - value2) >= float.Epsilon;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }
    }


    public abstract class ConditionalExpressionData
    {
    }

    public class Condition : ConditionalExpressionData
    {
        public IDPointer Pointer { get; set; }
        public ComparisonOperator Operator { get; set; }
        public int Value { get; set; }
    }

    public class LogicalExpression : ConditionalExpressionData
    {
        public ConditionalExpressionData Left { get; set; }
        public ComparisonOperator Operator { get; set; }
        public ConditionalExpressionData Right { get; set; }
    }

    public class ConditionalExpressionParser
    {
        private static Dictionary<string, ComparisonOperator> operatorMap = new Dictionary<string, ComparisonOperator>
    {
        { ">", ComparisonOperator.GreaterThan },
        { ">=", ComparisonOperator.GreaterThanOrEqual },
        { "<", ComparisonOperator.LessThan },
        { "<=", ComparisonOperator.LessThanOrEqual },
        { "==", ComparisonOperator.Equal },
        { "!=", ComparisonOperator.NotEqual },
        { "&&", ComparisonOperator.And },
        { "||", ComparisonOperator.Or }
    };

        public static ConditionalExpression Parse(string input, ArcaniaUnits arcaniaUnits)
        {
            var tokens = Tokenize(input);
            ConditionalExpressionData conditionalExpressionData = ParseExpression(tokens, arcaniaUnits);
            return new ConditionalExpression() {
                expression = conditionalExpressionData,
                rawExpression = input,
                humanExpression = TranslateExpression(input)
            };
        }

        public static string TranslateExpression(string expression)
        {
            return expression.Replace("&&", "and").Replace("||", "or");
        }

        private static List<string> Tokenize(string input)
        {
            var regex = new Regex(@"\w+|[><=!]+|\d+|&&|\|\||\(|\)");
            var matches = regex.Matches(input);
            var tokens = new List<string>();

            foreach (Match match in matches)
            {
                tokens.Add(match.Value);
            }

            return tokens;
        }

        private static ConditionalExpressionData ParseExpression(List<string> tokens, ArcaniaUnits arcaniaUnits)
        {
            var stack = new Stack<ConditionalExpressionData>();
            var operatorStack = new Stack<string>();

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                if (operatorMap.ContainsKey(token))
                {
                    while (operatorStack.Count > 0 && Precedence(operatorStack.Peek()) >= Precedence(token))
                    {
                        stack.Push(CreateLogicalExpression(stack, operatorStack.Pop()));
                    }
                    operatorStack.Push(token);
                }
                else if (token == "(")
                {
                    operatorStack.Push(token);
                }
                else if (token == ")")
                {
                    while (operatorStack.Peek() != "(")
                    {
                        stack.Push(CreateLogicalExpression(stack, operatorStack.Pop()));
                    }
                    operatorStack.Pop(); // Remove the '('
                }
                else if (Regex.IsMatch(token, @"\w+"))
                {
                    var variable = token;
                    ComparisonOperator op;
                    int value;

                    if (i + 1 < tokens.Count && operatorMap.ContainsKey(tokens[i + 1]))
                    {
                        op = operatorMap[tokens[++i]];
                        value = int.Parse(tokens[++i]);
                    }
                    else
                    {
                        op = ComparisonOperator.GreaterThan;
                        value = 0;
                    }

                    stack.Push(new Condition { Pointer = arcaniaUnits.GetOrCreateIdPointer(variable), Operator = op, Value = value });
                }
            }

            while (operatorStack.Count > 0)
            {
                stack.Push(CreateLogicalExpression(stack, operatorStack.Pop()));
            }

            return stack.Pop();
        }

        private static LogicalExpression CreateLogicalExpression(Stack<ConditionalExpressionData> stack, string op)
        {
            var right = stack.Pop();
            var left = stack.Pop();
            return new LogicalExpression { Left = left, Operator = operatorMap[op], Right = right };
        }

        private static int Precedence(string op)
        {
            switch (op)
            {
                case "&&": return 1;
                case "||": return 0;
                default: return 2; // Comparison operators
            }
        }
    }
}