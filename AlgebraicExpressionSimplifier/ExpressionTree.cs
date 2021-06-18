﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MathematicalExpressionCalculator
{
    public class ExpressionTree : IExpression, IEquatable<ExpressionTree>, IExpressionContextHolder<ExpressionTree>
    {
        public ExpressionContext Context { get; }
        public IExpression Left { get; set; }
        public IExpression Right { get; set; }
        public Operation Operation { get; set; }

        public ExpressionTree(IExpression left, Operation operation, IExpression right, ExpressionContext context)
        {
            Context = context;
            Left = left;
            Right = right;
            Operation = operation;
        }

        public ExpressionTree WithContext(ExpressionContext context)
        {
            IExpression ep1 = Left.WithContext(context);
            IExpression ep2 = Right.WithContext(context);
            return new ExpressionTree(ep1, Operation, ep2, context);
        }

        IExpression IExpressionContextHolder<IExpression>.WithContext(ExpressionContext context)
        {
            return WithContext(context);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var str1 = Left.ToString();
            var str2 = Right.ToString();

            if (Operation == Operation.Plus)
            {
                // 左右操作数均不加括号
                sb.Append(str1);
                sb.Append("+");
                sb.Append(str2);
            }
            else if (Operation == Operation.Minus)
            {
                // 左操作数不加括号，右操作数是乘除幂不加括号
                sb.Append(str1);

                sb.Append("-");

                if ((Right is Polynomial poly2 && poly2.Count == 1) ||
                    (Right is ExpressionTree tree2 && (tree2.Operation == Operation.Times || tree2.Operation == Operation.Divide || tree2.Operation == Operation.Power)))
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
            else if (Operation == Operation.Times)
            {
                // 左操作数是乘除幂不加括号，右操作数是乘除幂不加括号
                if ((Left is Polynomial poly1 && poly1.Count == 1) ||
                    (Left is ExpressionTree tree1 && (tree1.Operation == Operation.Times || tree1.Operation == Operation.Divide || tree1.Operation == Operation.Power)))
                {
                    sb.Append(str1);
                }
                else
                {
                    sb.Append("(");
                    sb.Append(str1);
                    sb.Append(")");
                }

                // sb.Append("*");

                if ((Right is Polynomial poly2 && poly2.Count == 1) ||
                    (Right is ExpressionTree tree2 && (tree2.Operation == Operation.Times || tree2.Operation == Operation.Divide || tree2.Operation == Operation.Power)))
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
            else if (Operation == Operation.Divide)
            {
                // 左操作数是乘除幂不加括号，右操作数是幂不加括号
                if ((Left is Polynomial poly1 && poly1.Count == 1) ||
                    (Left is ExpressionTree tree1 && (tree1.Operation == Operation.Times || tree1.Operation == Operation.Divide || tree1.Operation == Operation.Power)))
                {
                    sb.Append(str1);
                }
                else
                {
                    sb.Append("(");
                    sb.Append(str1);
                    sb.Append(")");
                }

                sb.Append("/");

                if (Right is Polynomial poly2 && (poly2.IsSymbol || poly2.IsNumber) ||
                    (Right is ExpressionTree tree2 && tree2.Operation == Operation.Power))
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
            else if (Operation == Operation.Power)
            {
                // 左操作数是幂不加括号，右操作数必加括号
                if ((Left is Polynomial poly1 && poly1.Count == 1) ||
                    (Left is ExpressionTree tree1 && tree1.Operation == Operation.Power))
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

                if (Right is Polynomial poly2 && (poly2.IsSymbol || poly2.IsNumber))
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
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ExpressionTree);
        }

        public bool Equals(ExpressionTree other)
        {
            return other != null &&
                   EqualityComparer<ExpressionContext>.Default.Equals(Context, other.Context) &&
                   EqualityComparer<IExpression>.Default.Equals(Left, other.Left) &&
                   EqualityComparer<IExpression>.Default.Equals(Right, other.Right) &&
                   Operation == other.Operation;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Context, Left, Right, Operation);
        }

        public static bool operator ==(ExpressionTree left, ExpressionTree right)
        {
            return EqualityComparer<ExpressionTree>.Default.Equals(left, right);
        }

        public static bool operator !=(ExpressionTree left, ExpressionTree right)
        {
            return !(left == right);
        }
    }
}
