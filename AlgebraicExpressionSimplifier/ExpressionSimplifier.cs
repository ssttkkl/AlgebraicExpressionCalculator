using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MathematicalExpressionCalculator
{
    public static class ExpressionSimplifier
    {
        public static IExpression Simplify(this IExpression ep)
        {
            if (ep is ExpressionTree)
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
                    if (ep2.TryGetAsNumber(out RationalNumber num)) // 除以数字
                        return ep1 / num;
                    else if (ep2.TryGetAsMonomial(out num, out UnitMonomial mono)) // 除以单项式（系数相除，变量幂数取相反数后相乘）
                        return ep1 / num * new Polynomial(1, mono.Reciprocal());
                    else // 代换
                    {
                        ExpressionTree sub = new ExpressionTree(ep1, Operation.Divide, ep2, context);
                        Symbol sy = tr.Context.Symbol(tr.ToString());
                        substitution[sy] = sub;
                        return new Polynomial(sy, 1);
                    }
                case Operation.Power:
                    if (ep2.TryGetAsNumber(out num) && num.IsInteger) // 幂为整数
                        return ep1.Power((BigInteger)num);
                    else // 代换
                    {
                        ExpressionTree sub = new ExpressionTree(ep1, Operation.Power, ep2, context);
                        Symbol sy = context.Symbol(sub.ToString());
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
                if (poly.TryGetAsSymbol(out Symbol sy) && substitution.TryGetValue(sy, out IExpression sub))
                    return sub;
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
        public static IExpression WithAssignment(this IExpression ep)
        {
            if (ep is Polynomial poly)
            {
                if (poly.TryGetAsSymbol(out var sy))
                {
                    if (sy.Assignment != null)
                        return sy.Assignment;
                    else
                        return poly;
                }
                else
                {
                    ep = poly.BuildExpressionTree();
                    if (ep is Polynomial)
                    {
                        // ep必为纯数字
                        return ep;
                    }
                }
            }

            var tree = ep as ExpressionTree;
            return new ExpressionTree(
                    tree.Left.WithAssignment(),
                    tree.Operation,
                    tree.Right.WithAssignment(),
                    tree.Context
            );
        }
    }
}
