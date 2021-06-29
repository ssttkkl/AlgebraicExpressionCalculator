using System.Linq;

namespace MathematicalExpressionCalculator
{
    public static class ExpressionTreeBuilder
    {
        public static IExpression BuildExpressionTree(this Polynomial t)
        {
            // 若该多项式只有一个元素（数或变量），则返回值仍为Polynomial类型
            Polynomial poly = new Polynomial(t);
            if (poly.Count == 1)
            {
                var item = poly.Single();
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

        private static IExpression BuildExpressionTree(RationalNumber coef, UnitMonomial mono)
        {
            if (coef.IsOne)
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
        private static IExpression BuildExpressionTree(UnitMonomial mono)
        {
            if (mono.Count == 0)
                return new Polynomial(1, mono.Context);
            else if (mono.Count == 1)
            {
                var item = mono.Single();
                return BuildExpressionTree(item.Key, item.Value);
            }
            else
            {
                var item = mono.First();
                mono.Remove(item.Key);
                var ep1 = BuildExpressionTree(item.Key, item.Value);
                var ep2 = BuildExpressionTree(mono);
                return new ExpressionTree(ep1, Operation.Times, ep2, mono.Context);
            }
        }
        private static IExpression BuildExpressionTree(Symbol sy, RationalNumber pow)
        {
            if (pow.IsOne)
                return new Polynomial(sy, 1);
            else
            {
                var ep1 = new Polynomial(sy, 1);
                var ep2 = new Polynomial(pow, sy.Context);
                return new ExpressionTree(ep1, Operation.Power, ep2, sy.Context);
            }
        }
    }
}
