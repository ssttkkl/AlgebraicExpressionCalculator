using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MathematicalExpressionCalculator
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

        public static IExpression Parse(string text, ExpressionContext context)
        {
            // REF: https://www.cnblogs.com/bianchengzhuji/p/10679924.html

            List<(string, int)> elements = SplitText(text);

            Stack<IExpression> eps = new Stack<IExpression>();
            Stack<string> ops = new Stack<string>();

            // 头尾加上括号
            ops.Push("(");
            elements.Add((")", 3));

            foreach ((string, int) ele in elements)
            {
                switch (ele.Item2)
                {
                    case 0: // 数字
                        RationalNumber num = RationalNumber.Parse(ele.Item1);
                        eps.Push(new Polynomial(num, context));
                        break;
                    case 1: // 变量
                        Symbol sy = context.Symbol(ele.Item1);
                        eps.Push(new Polynomial(sy, 1));
                        break;
                    case 2: // 算符
                        while (true)
                        {
                            if (ops.Count == 0)
                                break;

                            string op1 = ops.Peek();
                            string op2 = ele.Item1;
                            // 对于非乘方的运算，只要栈顶运算符**大于等于**当前运算符就弹栈；
                            // 对于乘方运算，只要栈顶运算符**大于**当前运算符就弹栈；
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
                    case 3: // 括号
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

            return eps.Count == 1 ? eps.Peek() : throw new ArgumentException("表达式不合法");
        }

        private static List<(string, int)> SplitText(string text)
        {
            List<(string, int)> list = new List<(string, int)>();
            StringBuilder numberBuilder = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsDigit(c) || c == '.') // 数字的一部分
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

                    if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^') // 算符
                    {
                        // 若-是负号而不是算符
                        if (c == '-' && (list.Count == 0 || (list[^1].Item2 != 0 && list[^1].Item2 != 1 && list[^1].Item2 != 3)))
                        {
                            list.Add(("-1", 0));
                            list.Add(("*", 2));
                        }
                        else
                        {
                            list.Add((c.ToString(), 2));
                        }
                    }
                    else if (c == '(') // 左括号
                    {
                        // 若两个元素间省略了乘号
                        if (list.Count > 0 && (list[^1].Item2 == 0 || list[^1].Item2 == 1 || list[^1].Item1 == ")"))
                        {
                            list.Add(("*", 2));
                        }
                        list.Add(("(", 3));
                    }
                    else if (c == ')') // 右括号
                    {
                        list.Add((")", 3));
                    }
                    else if (c == '{') // 大括号包裹的变量名
                    {
                        // 若两个元素间省略了乘号
                        if (list.Count > 0 && (list[^1].Item2 == 0 || list[^1].Item2 == 1 || list[^1].Item1 == ")"))
                        {
                            list.Add(("*", 2));
                        }

                        int j = i;
                        while (text[j] != '}')
                        {
                            j++;
                        }
                        list.Add((text.Substring(i + 1, j - i - 1), 1));
                        i = j;
                    }
                    else // 单个字母视为变量名
                    {
                        // 若两个元素间省略了乘号
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
