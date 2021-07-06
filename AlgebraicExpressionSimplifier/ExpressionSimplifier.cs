using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace MathematicalExpressionCalculator
{
    public static class ExpressionSimplifier
    {
        enum SubstitutionType
        {
            Divide, Power
        }

        public static IExpression Simplify(this IExpression ep)
        {
            if (ep is ExpressionTree)
            {
                var context = ep.Context;
                var subContext = new ExpressionContext();
                var substitution = new Dictionary<Symbol, IExpression>();

                var poly = Polynomialize(ep.WithContext(subContext), subContext, substitution);
                poly = MergePolynomial(poly, substitution);
                return RevertSubstitution(poly.EnsureTree(), substitution).WithContext(context);
            }
            else
            {
                return ep;
            }
        }

        // 判断ep1与ep2是否成比例，若是则直接返回比例系数
        private static bool IsPolynomializeRational(Polynomial ep1, Polynomial ep2, out RationalNumber ratio)
        {
            ratio = 0;
            if (ep1.Count != ep2.Count)
                return false;
            foreach (var item in ep1)
            {
                var pCoef = item.Value;
                if (!ep2.TryGetValue(item.Key, out var qCoef))
                    return false;
                if (ratio == 0)
                    ratio = pCoef / qCoef;
                else if (pCoef / qCoef != ratio)
                    return false;
            }
            return true;
        }

        private static Polynomial Polynomialize(IExpression ep, ExpressionContext context, IDictionary<Symbol, IExpression> substitution)
        {
            if (ep is Polynomial poly)
                return poly;

            var tr = (ExpressionTree)ep;

            if (tr.Operation == Operation.Plus)
            {
                Polynomial ep1 = Polynomialize(tr.Left, context, substitution);
                Polynomial ep2 = Polynomialize(tr.Right, context, substitution);
                return ep1 + ep2;
            }
            else if (tr.Operation == Operation.Minus)
            {
                Polynomial ep1 = Polynomialize(tr.Left, context, substitution);
                Polynomial ep2 = Polynomialize(tr.Right, context, substitution);
                return ep1 - ep2;
            }
            else if (tr.Operation == Operation.Times)
            {
                Polynomial ep1 = Polynomialize(tr.Left, context, substitution);
                if (ep1.TryGetAsNumber(out var num) && num.IsZero)
                    return new Polynomial(0, context);

                Polynomial ep2 = Polynomialize(tr.Right, context, substitution);
                if (ep2.TryGetAsNumber(out num) && num.IsZero)
                    return new Polynomial(0, context);

                return ep1 * ep2;
            }
            else if (tr.Operation == Operation.Divide)
            {
                Polynomial ep1 = Polynomialize(tr.Left, context, substitution);
                if (ep1.TryGetAsNumber(out var num) && num.IsZero)
                    return new Polynomial(0, context);

                Polynomial ep2 = Polynomialize(tr.Right, context, substitution);
                if (ep2.TryGetAsNumber(out num) && num.IsZero)
                    throw new DivideByZeroException();

                // 提取ep1, ep2中所有项的公因式（UnitMonomial），并约去
                UnitMonomial gblSy1 = null, gblSy2 = null;
                foreach (var mono in ep1.Keys)
                {
                    if (gblSy1 == null)
                        gblSy1 = new UnitMonomial(mono);
                    else
                        gblSy1 = gblSy1.Intersect(mono);
                }
                if (gblSy1 == null)
                    gblSy1 = new UnitMonomial(context);

                foreach (var mono in ep2.Keys)
                {
                    if (gblSy2 == null)
                        gblSy2 = new UnitMonomial(mono);
                    else
                        gblSy2 = gblSy2.Intersect(mono);
                }
                if (gblSy2 == null)
                    gblSy2 = new UnitMonomial(context);

                UnitMonomial gblSy = gblSy1.Intersect(gblSy2);
                if (gblSy.Count > 0)
                {
                    ep1 /= gblSy;
                    ep2 /= gblSy;
                }

                // 判断ep1与ep2是否成比例，若是则直接返回比例系数
                if (IsPolynomializeRational(ep1, ep2, out num))
                    return new Polynomial(num, context);
                if (ep2.TryGetAsNumber(out num)) // 若是除以数字
                    return ep1 / num;
                else if (ep2.TryGetAsMonomial(out num, out UnitMonomial mono)) // 若是除以单项式，则系数相除，变量幂数取反后相乘
                    return ep1 / num * new Polynomial(1, mono.Reciprocal());
                else
                {
                    var subb = ep2;
                    var syy = context.Symbol(subb.ToString());
                    substitution[syy] = subb;
                    return ep1 * new Polynomial(syy, -1);
                }
            }
            else if (tr.Operation == Operation.Power)
            {
                if (tr.Left is ExpressionTree tree1 && tree1.Operation == Operation.Power) // 若底也是幂运算，把两个指数相乘后递归处理
                {
                    Polynomial epLR = Polynomialize(tree1.Right, context, substitution);
                    Polynomial epR = Polynomialize(tr.Right, context, substitution);

                    ExpressionTree merged = new ExpressionTree(tree1.Left, Operation.Power, epLR * epR, context);
                    return Polynomialize(merged, context, substitution);
                }

                Polynomial ep1 = Polynomialize(tr.Left, context, substitution);
                if (ep1.TryGetAsNumber(out var num) && (num.IsOne || num.IsZero)) // 若底数为0或1
                    return new Polynomial(num, context);

                Polynomial ep2 = Polynomialize(tr.Right, context, substitution);
                if (ep2.TryGetAsNumber(out num)) // 若幂为数字
                {
                    if (num == 0) // 幂为0
                        return new Polynomial(1, context);
                    if (num.Sign > 0 && num.IsInteger) // 幂为正整数
                        return ep1.Power((BigInteger)num);
                    else if (ep1.TryGetAsMonomial(out var coef, out var mono)) // 幂为其他数字，底为单项式
                    {
                        if (coef.IsOne) // 单项式系数为1
                            return new Polynomial(1, mono.Power(num));
                        else if (num.IsInteger) // 幂为（负）整数
                            return new Polynomial(coef.Power((BigInteger)num), mono.Power(num));
                        else if (mono.Count == 0) // 幂为分数，底为数字，代换
                        {
                            ExpressionTree subb = new ExpressionTree(ep1, Operation.Power, ep2, context);
                            Symbol syy = context.Symbol(subb.ToString());
                            substitution[syy] = subb;
                            return new Polynomial(syy, 1);
                        }
                        else // 幂为分数，幂代换，底乘方
                        {
                            var ep11 = new ExpressionTree(new Polynomial(coef, context), Operation.Power, ep2, context);
                            var ep22 = new Polynomial(1, mono.Power(num));
                            ExpressionTree subb = new ExpressionTree(ep11, Operation.Times, ep22, context);
                            Symbol syy = context.Symbol(subb.ToString());
                            substitution[syy] = subb;
                            return new Polynomial(syy, 1);
                        }
                    }
                }

                ExpressionTree sub = new ExpressionTree(ep1, Operation.Power, ep2, context);
                Symbol sy = context.Symbol(sub.ToString());
                substitution[sy] = sub;
                return new Polynomial(sy, 1);
            }

            throw new NotSupportedException();
        }
        private static Polynomial MergePolynomial(Polynomial poly, IDictionary<Symbol, IExpression> substitution)
        {
            var context = poly.Context;
            var se = new List<(RationalNumber coef, UnitMonomial up, UnitMonomial down)>(); // 系数，分子，分母
            foreach (var item in poly)
            {
                var mono = item.Key;

                // 合并底数相同的幂
                var pow = new Dictionary<Symbol, Polynomial>();
                foreach (var item2 in mono)
                {
                    Symbol syL;
                    Polynomial polyR;

                    // 如果是由幂运算产生的代换，提取底数
                    if (substitution.TryGetValue(item2.Key, out IExpression sub) &&
                        sub is ExpressionTree tree && tree.Operation == Operation.Power)
                    {
                        if (!(tree.Left is Polynomial polyL && polyL.TryGetAsSymbol(out syL)))
                        {
                            syL = context.Symbol(tree.Left.ToString());
                            substitution[syL] = tree.Left;
                        }

                        if (tree.Right is ExpressionTree)
                        {
                            var syR = context.Symbol(tree.Right.ToString());
                            substitution[syR] = tree.Right;
                            polyR = new Polynomial(syR, 1) * new Polynomial(item2.Value, context);
                        }
                        else
                        {
                            polyR = (tree.Right as Polynomial) * new Polynomial(item2.Value, context);
                        }
                    }
                    // 否则直接处理
                    else
                    {
                        syL = item2.Key;
                        polyR = new Polynomial(item2.Value, context);
                    }
                    if (!pow.ContainsKey(syL))
                        pow[syL] = new Polynomial(context);
                    pow[syL] += polyR;
                }

                mono = new UnitMonomial(context);
                foreach (var item2 in pow)
                {
                    if (item2.Value.Count != 0)
                    {
                        var ep1 = item2.Key;
                        var ep2 = item2.Value;

                        if (ep2.TryGetAsNumber(out var num))
                        {
                            mono[ep1] = num;
                        }
                        else
                        {
                            ep2 = MergePolynomial(ep2, substitution); // 递归处理指数部分
                            var sub = new ExpressionTree(new Polynomial(ep1, 1), Operation.Power, ep2, context);
                            var subSy = context.Symbol(sub.ToString());
                            substitution[subSy] = sub;
                            mono[subSy] = 1;
                        }
                    }
                }

                // 提取分子分母（按照幂数是否大于0）
                var up = new UnitMonomial(context);
                var down = new UnitMonomial(context);

                foreach (var item2 in mono)
                {
                    if (item2.Value > 0)
                        up[item2.Key] = item2.Value;
                    else
                        down[item2.Key] = -item2.Value;
                }
                se.Add((item.Value, up, down));
            }

            // 合并分母相同的项
            var p = new Polynomial(context);
            foreach (var group in se.GroupBy(it => it.down))
            {
                var ep1 = new Polynomial(context);
                foreach (var item in group)
                {
                    ep1[item.up] = item.coef;
                }

                // 若分子只有一项或没有分母，则直接将分子加到总多项式
                if (group.Key.Count == 0)
                    p += ep1;
                else if (group.Count() == 1)
                    p += ep1 * group.Key.Power(-1);
                else
                {
                    // 判断分母是否有与分子成比例的代换变量，若有则约去
                    foreach (var item in group.Key)
                    {
                        if (substitution.TryGetValue(item.Key, out var sub) &&
                            sub is Polynomial pSub && IsPolynomializeRational(ep1, pSub, out var ratio))
                        {
                            if (item.Value == 1)
                                group.Key.Remove(item.Key);
                            else
                                group.Key[item.Key] = item.Value - 1;
                            ep1 = new Polynomial(ratio, context);
                            break;
                        }
                    }

                    // 若约分后没有分母，则直接将分子加到总多项式
                    if (group.Key.Count == 0)
                        p += ep1;
                    else
                    {
                        // 将整个分式代换
                        var ep2 = new Polynomial(1, group.Key);
                        var sub = new ExpressionTree(ep1, Operation.Divide, ep2, context);
                        var sy = context.Symbol(sub.ToString());
                        substitution[sy] = sub;
                        p[new UnitMonomial(sy, 1)] = 1;
                    }
                }
            }
            return p;
        }
        private static IExpression RevertSubstitution(IExpression ep, IDictionary<Symbol, IExpression> substitution)
        {
            if (ep is Polynomial poly)
            {
                if (poly.TryGetAsSymbol(out Symbol sy) && substitution.TryGetValue(sy, out IExpression sub))
                    return RevertSubstitution(sub.EnsureTree(), substitution);
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
                    ep = poly.EnsureTree();
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
