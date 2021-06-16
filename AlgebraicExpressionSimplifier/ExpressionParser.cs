using System;
using System.Collections.Generic;
using System.Text;

namespace AlgebraicExpressionSimplifier
{
    public static class ExpressionParser
    {
        private static int OperatorPriority(string op)
        {
            if (op == "^")
                return 6;
            else if (op == "*" || op == "/")
                return 5;
            else if (op == "+" || op == "-")
                return 4;
            else if (op == "(")
                return 0;
            else
                throw new ArgumentException($"Illegal Operator: {op}");
        }

        public static IExpression Parse(string text)
        {
            // REF: https://www.cnblogs.com/bianchengzhuji/p/10679924.html

            List<(string, int)> elements = SplitText(text);

            ExpressionContext context = new ExpressionContext();

            Stack<IExpression> eps = new Stack<IExpression>();
            Stack<string> ops = new Stack<string>();

            // 头尾加上括号
            ops.Push("(");
            elements.Add((")", 3));

            foreach ((string, int) ele in elements)
            {
                switch (ele.Item2)
                {
                    case 0:
                        double num = double.Parse(ele.Item1);
                        eps.Push(new Polynomial(num, context));
                        break;
                    case 1:
                        Symbol sy = context.Symbol(ele.Item1);
                        eps.Push(new Polynomial(sy));
                        break;
                    case 2:
                        while (true)
                        {
                            if (ops.Count == 0)
                                break;

                            string op1 = ops.Peek();
                            string op2 = ele.Item1;
                            // 对于非乘方的运算，只要栈顶运算符大于等于当前运算符就弹栈；
                            // 对于乘方运算，只要栈顶运算符大于当前运算符就弹栈；
                            if (!((op2 == "^" && OperatorPriority(op1) > OperatorPriority(op2))
                                || (op2 != "^" && OperatorPriority(op1) >= OperatorPriority(op2))))
                                break;

                            IExpression ep2 = eps.Pop();
                            IExpression ep1 = eps.Pop();
                            switch (ops.Pop())
                            {
                                case "+":
                                    eps.Push(new ExpressionTree(ep1, Operation.Plus, ep2, context));
                                    break;
                                case "-":
                                    eps.Push(new ExpressionTree(ep1, Operation.Minus, ep2, context));
                                    break;
                                case "*":
                                    eps.Push(new ExpressionTree(ep1, Operation.Times, ep2, context));
                                    break;
                                case "/":
                                    eps.Push(new ExpressionTree(ep1, Operation.Divide, ep2, context));
                                    break;
                                case "^":
                                    eps.Push(new ExpressionTree(ep1, Operation.Power, ep2, context));
                                    break;
                                default:
                                    throw new ArgumentException();
                            }
                        }
                        ops.Push(ele.Item1);
                        break;
                    case 3:
                        if (ele.Item1 == "(")
                            ops.Push("(");
                        else if (ele.Item1 == ")")
                        {
                            while (ops.Peek() != "(")
                            {
                                IExpression ep2 = eps.Pop();
                                IExpression ep1 = eps.Pop();
                                switch (ops.Pop())
                                {
                                    case "+":
                                        eps.Push(new ExpressionTree(ep1, Operation.Plus, ep2, context));
                                        break;
                                    case "-":
                                        eps.Push(new ExpressionTree(ep1, Operation.Minus, ep2, context));
                                        break;
                                    case "*":
                                        eps.Push(new ExpressionTree(ep1, Operation.Times, ep2, context));
                                        break;
                                    case "/":
                                        eps.Push(new ExpressionTree(ep1, Operation.Divide, ep2, context));
                                        break;
                                    case "^":
                                        eps.Push(new ExpressionTree(ep1, Operation.Power, ep2, context));
                                        break;
                                    default:
                                        throw new ArgumentException();
                                }
                            }
                            ops.Pop(); // 弹出左括号
                        }
                        break;
                }
            }

            return eps.Count == 1 ? eps.Peek() : throw new ArgumentException("Illegal expression");
        }

        public static List<(string, int)> SplitText(string text)
        {
            List<(string, int)> list = new List<(string, int)>();
            StringBuilder numberBuilder = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsDigit(c) || c == '.') // part of a number
                {
                    numberBuilder.Append(c);
                }
                else
                {
                    if (numberBuilder.Length > 0)
                    {
                        string number = numberBuilder.ToString();
                        list.Add((number, 0));
                        numberBuilder.Clear();
                    }

                    if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^') // operator
                    {
                        if (list.Count == 0 || (c == '-' && list[^1].Item2 != 0 && list[^1].Item2 != 1))
                        {
                            list.Add(("-1", 0));
                            list.Add(("*", 2));
                        }
                        else
                        {
                            list.Add((c.ToString(), 2));
                        }
                    }
                    else if (c == '(')
                    {
                        if (list.Count > 0 && (list[^1].Item2 == 0 || list[^1].Item2 == 1 || list[^1].Item1 == ")"))
                        {
                            list.Add(("*", 2));
                        }
                        list.Add((c.ToString(), 3));
                    }
                    else if (c == ')')
                    {
                        list.Add((c.ToString(), 3));
                    }
                    else // symbol
                    {
                        if (list.Count > 0 && (list[^1].Item2 == 0 || list[^1].Item2 == 1 || list[^1].Item1 == ")"))
                        {
                            list.Add(("*", 2));
                        }
                        list.Add((c.ToString(), 1));
                    }
                }
            }

            if (numberBuilder.Length > 0)
            {
                string number = numberBuilder.ToString();
                list.Add((number, 0));
            }

            return list;
        }
    }
}
