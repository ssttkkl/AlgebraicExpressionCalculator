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
                        //ExpressionTree subb = new ExpressionTree(ep1, Operation.Divide, ep2, context);
                        //Symbol syy = tr.Context.Symbol(subb.ToString());
                        //substitution[syy] = subb;
                        //return new Polynomial(syy, 1);
                        var subb = ep2;
                        var syy = context.Symbol(subb.ToString());
                        substitution[syy] = subb;
                        return ep1 * new Polynomial(syy, -1);
                    }
                case Operation.Power:
                    if (ep2.TryGetAsNumber(out num))
                    {
                        if (num.Sign > 0 && num.IsInteger) // 幂为正整数
                            return ep1.Power((BigInteger)num);
                        else if (ep1.TryGetAsMonomial(out var coef, out var mono)) // 幂为其他数字，底为单项式
                        {
                            if (coef.IsOne) // 单项式系数为1
                            {
                                return new Polynomial(1, mono.Power(num));
                            }    
                            else if (num.IsInteger) // 幂为负整数
                            {
                                return new Polynomial(coef.Power((BigInteger)num), mono.Power(num));
                            }
                            else if (mono.Count == 0) // 幂为分数，底为数字
                            {
                                ExpressionTree subb = new ExpressionTree(ep1, Operation.Power, ep2, context);
                                Symbol syy = context.Symbol(subb.ToString());
                                substitution[syy] = subb;
                                return new Polynomial(syy, 1);
                            }
                            else
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

                    // 代换
                    ExpressionTree sub = new ExpressionTree(ep1, Operation.Power, ep2, context);
                    Symbol sy = context.Symbol(sub.ToString());
                    substitution[sy] = sub;
                    return new Polynomial(sy, 1);
                default:
                    throw new NotImplementedException();
            }
        }
        private static Polynomial MergePolynomial(Polynomial poly, IDictionary<Symbol, IExpression> substitution)
        {
            var se = new List<(RationalNumber coef, UnitMonomial up, Symbol down)>(); // 系数，分子，分母
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
                            syL = poly.Context.Symbol(tree.Left.ToString());
                            substitution[syL] = tree.Left;
                        }

                        if (tree.Right is ExpressionTree)
                        {
                            var syR = poly.Context.Symbol(tree.Right.ToString());
                            substitution[syR] = tree.Right;
                            polyR = new Polynomial(syR, 1);
                        }
                        else
                        {
                            polyR = tree.Right as Polynomial;
                        }
                    }
                    // 否则直接处理
                    else
                    {
                        syL = item2.Key;
                        polyR = new Polynomial(item2.Value, poly.Context);
                    }
                    if (!pow.ContainsKey(syL))
                        pow[syL] = new Polynomial(poly.Context);
                    pow[syL] += polyR;
                }

                mono = new UnitMonomial(poly.Context);
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
                            var sub = new ExpressionTree(new Polynomial(ep1, 1), Operation.Power, ep2, poly.Context);
                            var subSy = poly.Context.Symbol(sub.ToString());
                            substitution[subSy] = sub;
                            mono[subSy] = 1;
                        }
                    }
                }

                // 合并指数小于0的变量（分母）
                var up = new UnitMonomial(poly.Context);
                var down = new UnitMonomial(poly.Context);

                foreach (var item2 in mono)
                {
                    if (item2.Value > 0)
                        up[item2.Key] = item2.Value;
                    else
                        down[item2.Key] = -item2.Value;
                }

                Symbol sy = null;
                if (down.Count > 0)
                {
                    // 如果分母就只是一个变量，直接用这个变量
                    // 否则进行代换
                    if (!down.TryGetAsSymbol(out sy))
                    {
                        var sub = new Polynomial(1, down);
                        // var sub = MergePolynomial(new Polynomial(1, down), substitution); // 递归处理分母
                        sy = poly.Context.Symbol(sub.ToString());
                        substitution[sy] = sub;
                    }
                }
                se.Add((item.Value, up, sy));
            }

            // 合并分母相同的项
            var p = new Polynomial(poly.Context);
            foreach (var item in se.GroupBy(it => it.down))
            {
                var ep1 = new Polynomial(poly.Context);
                foreach (var item2 in item)
                {
                    ep1[item2.up] = item2.coef;
                }

                if (item.Key != null)
                {
                    // 代换
                    var ep2 = new Polynomial(item.Key, 1);
                    var sub = new ExpressionTree(ep1, Operation.Divide, ep2, poly.Context);
                    var sy = poly.Context.Symbol(sub.ToString());
                    substitution[sy] = sub;
                    p[new UnitMonomial(sy, 1)] = 1;
                }
                else
                {
                    p += ep1;
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
