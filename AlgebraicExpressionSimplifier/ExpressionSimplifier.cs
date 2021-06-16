using System;
using System.Collections.Generic;
using System.Text;

namespace AlgebraicExpressionSimplifier
{
    public static class ExpressionSimplifier
    {
        public static IExpression Simplify(this ExpressionTree ep)
        {
            var context = new ExpressionContext();
            var substitution = new Dictionary<Symbol, IExpression>();
            var poly = Polynomialize(ep.WithContext(context), substitution);
            var ep2 = poly.BuildExpressionTree();
            return RevertSubstitution(ep2, substitution);

        }

        private static Polynomial Polynomialize(IExpression ep, IDictionary<Symbol, IExpression> substitution)
        {
            if (ep is Polynomial)
                return (Polynomial)ep;

            var tr = (ExpressionTree)ep;
            Polynomial ep1 = Polynomialize(tr.Left, substitution);
            Polynomial ep2 = Polynomialize(tr.Right, substitution);

            switch (tr.Operation)
            {
                case Operation.Plus:
                    return ep1.Plus(ep2);
                case Operation.Minus:
                    return ep1.Minus(ep2);
                case Operation.Times:
                    return ep1.Times(ep2);
                case Operation.Divide:
                    if (ep2.TryGetAsNumber(out double num))
                        return ep1.Divide(num);
                    else if (ep2.TryGetAsMonomial(out num, out UnitMonomial mono))
                        return ep1.Divide(num).Times(new Polynomial(mono.Inv()));
                    else
                    {
                        ExpressionTree sub = new ExpressionTree(ep1, Operation.Divide, ep2, ep.Context);
                        Symbol sy = ep.Context.Symbol(tr.ToString());
                        substitution[sy] = sub;
                        return new Polynomial(sy);
                    }
                case Operation.Power:
                    if (ep2.TryGetAsNumber(out num))
                        return ep1.Power((int)num);
                    else
                    {
                        ExpressionTree sub = new ExpressionTree(ep1, Operation.Power, ep2, ep.Context);
                        Symbol sy = ep.Context.Symbol(tr.ToString());
                        substitution[sy] = sub;
                        return new Polynomial(sy);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private static IExpression RevertSubstitution(IExpression ep, IDictionary<Symbol, IExpression> substitution)
        {
            if (ep is Polynomial)
            {
                var poly = (Polynomial)ep;
                if (poly.TryGetAsSymbol(out Symbol sy) && substitution.TryGetValue(sy, out IExpression ans))
                    return ans;
                else
                    return poly;
            }
            else
            {
                var tree = (ExpressionTree)ep;
                return new ExpressionTree(
                    RevertSubstitution(tree.Left, substitution),
                    tree.Operation,
                    RevertSubstitution(tree.Right, substitution),
                    tree.Context);
            }
        }
    }
}
