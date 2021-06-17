using System;
using System.Collections;
using System.Linq;

namespace AlgebraicExpressionSimplifier
{
    public interface IExpression : IExpressionContextHolder<IExpression>
    {
        
    }

    public static class ExpressionExt
    {
        private static IExpression BuildExpressionTree(UnitMonomial mono)
        {
            if (mono.Count == 0)
                return new Polynomial(1, mono.Context);
            else if (mono.Count == 1)
            {
                var item = mono.Single();
                return new Polynomial(item.Key, item.Value);
            }
            else
            {
                var item = mono.First();
                mono.Remove(item.Key);
                var ep1 = new Polynomial(item.Key, item.Value);
                var ep2 = BuildExpressionTree(mono);
                return new ExpressionTree(ep1, Operation.Times, ep2, mono.Context);
            }
        }

        private static IExpression BuildExpressionTree(RationalNumber coef, UnitMonomial mono)
        {
            if (coef == 1)
                return BuildExpressionTree(mono);
            else if (mono.Count == 0)
                return new Polynomial(coef, mono.Context);
            else
                return new ExpressionTree(
                    new Polynomial(coef, mono.Context),
                    Operation.Times,
                    BuildExpressionTree(mono),
                    mono.Context);
        }

        public static IExpression BuildExpressionTree(this Polynomial t)
        {
            Polynomial poly = new Polynomial(t);
            if (poly.Count == 1)
            {
                var item = poly.First();
                return BuildExpressionTree(item.Value, item.Key);
            }
            else
            {
                var item = poly.First();
                poly.Remove(item.Key);
                var ep1 = BuildExpressionTree(item.Value, item.Key);
                var ep2 = BuildExpressionTree(poly);
                return new ExpressionTree(ep1, Operation.Plus, ep2, poly.Context);
            }
        }
    }
}
