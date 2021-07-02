using System.Linq;
using System;

namespace MathematicalExpressionCalculator
{
    public static class ExpressionTreeBuilder
    {
        public static IExpression EnsureTree(this IExpression t)
        {
            if (t is ExpressionTree tree)
                return tree.EnsureTree();
            else if (t is Polynomial poly)
                return poly.EnsureTree();
            else
                throw new NotSupportedException();
        }
        private static IExpression EnsureTree(this ExpressionTree t)
        {
            return new ExpressionTree(t.Left.EnsureTree(),
                t.Operation,
                t.Right.EnsureTree(),
                t.Context);
        }
        private static IExpression EnsureTree(this Polynomial t)
        {
            // 若该多项式只有零或一个元素（数或变量），则返回值仍为Polynomial类型
            if (t.Count == 0)
            {
                return t;
            }
            else if (t.Count == 1)
            {
                var item = t.Single();
                return EnsureTree(item.Value, item.Key);
            }
            else
            {
                Polynomial poly = new Polynomial(t);
                var item = poly.First();
                poly.Remove(item.Key);
                var ep1 = EnsureTree(item.Value, item.Key);
                var ep2 = EnsureTree(poly);
                return new ExpressionTree(ep1, Operation.Plus, ep2, poly.Context);
            }
        }

        private static IExpression EnsureTree(RationalNumber coef, UnitMonomial mono)
        {
            if (mono.Count == 0)
                return new Polynomial(coef, mono.Context);

            var posMono = new UnitMonomial(mono.Context);
            var negMono = new UnitMonomial(mono.Context);
            foreach (var item in mono)
            {
                if (item.Value.Sign > 0)
                    posMono[item.Key] = item.Value;
                else
                    negMono[item.Key] = -item.Value;
            }

            if (negMono.Count > 0)
            {
                var ep2 = EnsureTree(negMono);
                if (posMono.Count == 0)
                    return new ExpressionTree(new Polynomial(coef, mono.Context), Operation.Divide, ep2, mono.Context);
                else
                {
                    var ep1 = EnsureTree(posMono);
                    if (!coef.IsOne)
                        ep1 = new ExpressionTree(new Polynomial(coef, ep1.Context), Operation.Times, ep1, ep1.Context);
                    return new ExpressionTree(ep1, Operation.Divide, ep2, mono.Context);
                }
            }
            else
            {
                var ep1 = EnsureTree(posMono);
                if (!coef.IsOne)
                    ep1 = new ExpressionTree(new Polynomial(coef, ep1.Context), Operation.Times, ep1, ep1.Context);
                return ep1;
            }
        }

        private static IExpression EnsureTree(UnitMonomial mono)
        {
            if (mono.Count == 0)
                return new Polynomial(1, mono.Context);

            if (mono.Count == 1)
            {
                var item = mono.Single();
                return EnsureTree(item.Key, item.Value);
            }
            else
            {
                var item = mono.First();
                mono.Remove(item.Key);
                var ep1 = EnsureTree(item.Key, item.Value);
                var ep2 = EnsureTree(mono);
                return new ExpressionTree(ep1, Operation.Times, ep2, mono.Context);
            }
        }
        private static IExpression EnsureTree(Symbol sy, RationalNumber pow)
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
