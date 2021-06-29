using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathematicalExpressionCalculator
{
    public static class ExpressionToLaTeX
    {
        private static string ToLaTeX(this RationalNumber num)
        {
            if (num.Denominator == 1)
            {
                return num.Sign switch
                {
                    0 => "0",
                    1 => $"{num.Numerator}",
                    -1 => $"-{num.Numerator}",
                    _ => throw new InvalidOperationException("Sign must be 0, 1 or -1"),
                };
            }
            else
            {
                return num.Sign switch
                {
                    0 => "0",
                    1 => $"\\frac {{{num.Numerator}}} {{{num.Denominator}}}",
                    -1 => $"-\\frac {{{num.Numerator}}} {{{num.Denominator}}}",
                    _ => throw new InvalidOperationException("Sign must be 0, 1 or -1"),
                };
            }
        }

        private static string ToLaTeX(this Polynomial poly)
        {
            var sb = new StringBuilder();
            foreach (var item in poly)
            {
                if (sb.Length > 0 && item.Value.Sign > 0)
                    sb.Append("+");
                if (item.Key.Count == 0)
                    sb.Append(item.Value);
                else if (item.Value.IsMinusOne)
                    sb.Append("-");
                else if (!item.Value.IsOne)
                    sb.Append(item.Value);
                foreach (var item2 in item.Key)
                {
                    sb.Append(item2.Key.ToString());
                    if (!item2.Value.IsOne)
                    {
                        sb.Append("^");
                        sb.Append("{");
                        sb.Append(item2.Value);
                        sb.Append("}");
                    }
                }
            }
            return sb.ToString();
        }

        private static string ToLaTeX(this ExpressionTree tree)
        {
            var sb = new StringBuilder();
            var str1 = tree.Left.ToLaTeX();
            var str2 = tree.Right.ToLaTeX();

            if (tree.Operation == Operation.Plus)
            {
                // 左右操作数均不加括号
                sb.Append(str1);
                sb.Append("+");
                sb.Append(str2);
            }
            else if (tree.Operation == Operation.Minus)
            {
                // 左操作数不加括号，右操作数是乘除幂不加括号
                sb.Append(str1);

                sb.Append("-");

                if ((tree.Right is Polynomial poly2 && poly2.Count == 1) ||
                    (tree.Right is ExpressionTree tree2 && (tree2.Operation == Operation.Times || tree2.Operation == Operation.Divide || tree2.Operation == Operation.Power)))
                {
                    sb.Append(str2);
                }
                else
                {
                    sb.Append("(");
                    sb.Append(str2);
                    sb.Append(")");
                }
            }
            else if (tree.Operation == Operation.Times)
            {
                // 左操作数是乘除幂不加括号，右操作数是乘除幂不加括号
                if ((tree.Left is Polynomial poly1 && poly1.Count == 1) ||
                    (tree.Left is ExpressionTree tree1 && (tree1.Operation == Operation.Times || tree1.Operation == Operation.Divide || tree1.Operation == Operation.Power)))
                {
                    sb.Append(str1);
                }
                else
                {
                    sb.Append("(");
                    sb.Append(str1);
                    sb.Append(")");
                }

                // 右操作数是单项式且系数不为1时加上乘号
                if (tree.Right is Polynomial poly22 && poly22.Count == 1 && !poly22.Single().Value.IsOne)
                {
                    sb.Append(" \\times ");
                }

                if ((tree.Right is Polynomial poly2 && poly2.Count == 1) ||
                    (tree.Right is ExpressionTree tree2 && (tree2.Operation == Operation.Times || tree2.Operation == Operation.Divide || tree2.Operation == Operation.Power)))
                {
                    sb.Append(str2);
                }
                else
                {
                    sb.Append("(");
                    sb.Append(str2);
                    sb.Append(")");
                }
            }
            else if (tree.Operation == Operation.Divide)
            {
                sb.Append("\\frac {");
                sb.Append(str1);
                sb.Append("} {");
                sb.Append(str2);
                sb.Append("} ");
            }
            else if (tree.Operation == Operation.Power)
            {
                if (tree.Left is Polynomial poly1 && (poly1.IsSymbol || poly1.IsNumber))
                {
                    sb.Append(str1);
                }
                else
                {
                    sb.Append("(");
                    sb.Append(str1);
                    sb.Append(")");
                }

                sb.Append("^");
                sb.Append("{");
                sb.Append(str2);
                sb.Append("}");
            }
            return sb.ToString();
        }

        public static string ToLaTeX(this IExpression ep)
        {
            if (ep is Polynomial p)
                return p.ToLaTeX();
            else if (ep is ExpressionTree t)
                return t.ToLaTeX();
            else
                throw new NotSupportedException();
        }
    }
}
