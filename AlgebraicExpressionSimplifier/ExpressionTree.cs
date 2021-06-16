using System;
using System.Collections.Generic;

namespace AlgebraicExpressionSimplifier
{
    public class ExpressionTree : IExpression, IEquatable<ExpressionTree>
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
            IExpression ep1;
            if (Left is ExpressionTree)
                ep1 = ((ExpressionTree)Left).WithContext(context);
            else
                ep1 = ((Polynomial)Left).WithContext(context);

            IExpression ep2;
            if (Right is ExpressionTree)
                ep2 = ((ExpressionTree)Right).WithContext(context);
            else
                ep2 = ((Polynomial)Right).WithContext(context);

            return new ExpressionTree(ep1, Operation, ep2, context);
        }

        public override string ToString()
        {
            string op = "";
            switch (Operation)
            {
                case Operation.Plus:
                    op = "+";
                    break;
                case Operation.Minus:
                    op = "-";
                    break;
                case Operation.Times:
                    op = "*";
                    break;
                case Operation.Divide:
                    op = "/";
                    break;
                case Operation.Power:
                    op = "^";
                    break;
            }
            return $"({Left}{op}{Right})";
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
