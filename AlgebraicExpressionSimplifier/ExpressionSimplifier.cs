using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AlgebraicExpressionSimplifier
{
    public static class ExpressionSimplifier
    {
        public static IExpression WithConstraint(this IExpression ep)
        {
            if (ep is Polynomial poly)
            {
                ep = poly.BuildExpressionTree();
                if (ep is Polynomial poly2)
                {
                    // poly2必为纯变量或纯数字
                    if (poly2.TryGetAsSymbol(out var sy) && sy.Constraint != null)
                    {
                        return sy.Constraint;
                    }
                    else
                    {
                        return poly2;
                    }
                }
            }

            var tree = ep as ExpressionTree;
            return new ExpressionTree(
                    tree.Left.WithConstraint(),
                    tree.Operation,
                    tree.Right.WithConstraint(),
                    tree.Context
            );
        }
        public static IExpression Simplify(this IExpression ep)
        {
            if (ep is ExpressionTree tree)
            {
                var context = ep.Context;
                var subContext = new ExpressionContext();
                var substitution = new Dictionary<Symbol, IExpression>();
                var poly = Polynomialize(ep.WithContext(subContext), subContext, substitution);
                var ep2 = poly.BuildExpressionTree();
                return RevertSubstitution(ep2, substitution).WithContext(context);
            }
            else
            {
                return ep;
            }
        }

        private static Polynomial Polynomialize(IExpression ep, ExpressionContext context, IDictionary<Symbol, IExpression> substitution)
        {
            if (ep is Polynomial poly)
                return poly;

            var tr = (ExpressionTree)ep;
            Polynomial ep1 = Polynomialize(tr.Left, context, substitution);
            Polynomial ep2 = Polynomialize(tr.Right, context, substitution);

            switch (tr.Operation)
            {
                case Operation.Plus:
                    return ep1 + ep2;
                case Operation.Minus:
                    return ep1 - ep2;
                case Operation.Times:
                    return ep1 * ep2;
                case Operation.Divide:
                    if (ep2.TryGetAsNumber(out RationalNumber num))
                        return ep1 / num;
                    else if (ep2.TryGetAsMonomial(out num, out UnitMonomial mono))
                        return ep1 / num * new Polynomial(mono.Inv(), 1);
                    else
                    {
                        ExpressionTree sub = new ExpressionTree(ep1, Operation.Divide, ep2, context);
                        Symbol sy = tr.Context.Symbol(tr.ToString());
                        substitution[sy] = sub;
                        return new Polynomial(sy, 1);
                    }
                case Operation.Power:
                    if (ep2.TryGetAsNumber(out num))
                        return ep1.Power((BigInteger)num);
                    else
                    {
                        ExpressionTree sub = new ExpressionTree(ep1, Operation.Power, ep2, context);
                        Symbol sy = context.Symbol(tr.ToString());
                        substitution[sy] = sub;
                        return new Polynomial(sy, 1);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private static IExpression RevertSubstitution(IExpression ep, IDictionary<Symbol, IExpression> substitution)
        {
            if (ep is Polynomial poly)
            {
                if (poly.TryGetAsSymbol(out Symbol sy) && substitution.TryGetValue(sy, out IExpression ans))
                    return ans;
                else
                    return poly;
            }
            else if (ep is ExpressionTree tree)
            {
                return new ExpressionTree(
                    RevertSubstitution(tree.Left, substitution),
                    tree.Operation,
                    RevertSubstitution(tree.Right, substitution),
                    tree.Context);
            }
            throw new NotSupportedException();
        }
    }
}
