﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;

namespace MathematicalExpressionCalculator
{
    public class Polynomial : Dictionary<UnitMonomial, RationalNumber>, IExpression, IEquatable<Polynomial>, IExpressionContextHolder<Polynomial>
    {
        public ExpressionContext Context { get; }

        public Polynomial(ExpressionContext context)
        {
            Context = context;
        }

        public Polynomial(Polynomial poly) : base(poly)
        {
            Context = poly.Context;
        }

        public Polynomial(RationalNumber number, ExpressionContext context)
        {
            Context = context;
            this[new UnitMonomial(context)] = number;
        }

        public Polynomial(Symbol sy, RationalNumber power)
        {
            Context = sy.Context;
            this[new UnitMonomial(sy, power)] = 1;
        }

        public Polynomial(RationalNumber coef, UnitMonomial mono)
        {
            Context = mono.Context;
            this[mono] = coef;
        }

        public void Trim()
        {
            foreach (var key in Keys.ToArray())
            {
                if (TryGetValue(key, out RationalNumber val) && val.IsZero)
                {
                    Remove(key);
                }
            }
        }

        public Polynomial WithContext(ExpressionContext context)
        {
            var poly = new Polynomial(context);
            foreach (var item in this)
            {
                poly[item.Key.WithContext(context)] = item.Value;
            }
            return poly;
        }
        IExpression IExpressionContextHolder<IExpression>.WithContext(ExpressionContext context)
        {
            return WithContext(context);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Polynomial);
        }

        public bool Equals(Polynomial other)
        {
            if (other == null || Count != other.Count)
                return false;

            foreach (var item in this)
            {
                if (!other.TryGetValue(item.Key, out var val) || val != item.Value)
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var item in this)
            {
                hash ^= HashCode.Combine(item.Key, item.Value);
            }
            return hash;
        }

        public static bool operator ==(Polynomial left, Polynomial right)
        {
            return EqualityComparer<Polynomial>.Default.Equals(left, right);
        }

        public static bool operator !=(Polynomial left, Polynomial right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            if (Count == 0)
                return "0";
            
            var sb = new StringBuilder();
            foreach (var item in this)
            {
                if (sb.Length > 0 && item.Value.Sign > 0)
                    sb.Append("+");
                if (item.Key.Count == 0)
                    sb.Append(item.Value);
                else if (item.Value.IsMinusOne)
                    sb.Append("-");
                else if (!item.Value.IsOne)
                    sb.Append(item.Value);
                foreach (var item2 in item.Key)
                {
                    sb.Append(item2.Key.ToString());
                    if (!item2.Value.IsOne)
                    {
                        sb.Append("^");
                        if (item2.Value.Sign < 0 || !item2.Value.IsInteger)
                        {
                            sb.Append("(");
                            sb.Append(item2.Value);
                            sb.Append(")");
                        }
                        else
                            sb.Append(item2.Value);
                    }
                }
            }
            return sb.ToString();
        }
        public bool IsNumber => Count == 0 || (Count == 1 && this.Single().Key.Count == 0);
        public bool TryGetAsNumber(out RationalNumber number)
        {
            if (Count == 0)
            {
                number = 0;
                return true;
            }
            if (Count != 1)
            {
                number = 0;
                return false;
            }

            var item = this.Single();
            number = item.Value;
            return item.Key.Count == 0;
        }
        public bool IsSymbol => Count == 1 && this.Single().Key.IsSymbol;
        public bool TryGetAsSymbol(out Symbol symbol)
        {
            if (Count != 1)
            {
                symbol = null;
                return false;
            }

            var item = this.Single();
            return item.Key.TryGetAsSymbol(out symbol) && item.Value == 1;
        }
        public bool TryGetAsMonomial(out RationalNumber coef, out UnitMonomial monomial)
        {
            if (Count != 1)
            {
                coef = 0;
                monomial = null;
                return false;
            }

            var item = this.Single();
            coef = item.Value;
            monomial = item.Key;
            return true;
        }

        public static Polynomial operator +(Polynomial x, Polynomial y)
        {
            if (x.Context != y.Context)
                throw new DifferentContextException();

            var poly = new Polynomial(x);
            foreach (var item in y)
            {
                if (!poly.TryGetValue(item.Key, out RationalNumber val))
                    val = 0;
                poly[item.Key] = val + item.Value;
            }
            poly.Trim();
            return poly;
        }

        public static Polynomial operator -(Polynomial x, Polynomial y)
        {
            if (x.Context != y.Context)
                throw new DifferentContextException();

            var poly = new Polynomial(x);
            foreach (var item in y)
            {
                if (!poly.TryGetValue(item.Key, out RationalNumber val))
                    val = 0;
                poly[item.Key] = val - item.Value;
            }
            poly.Trim();
            return poly;
        }

        public static Polynomial operator *(Polynomial x, Polynomial y)
        {
            if (x.Context != y.Context)
                throw new DifferentContextException();

            var poly = new Polynomial(x.Context);
            foreach (var item in x)
            {
                foreach (var item2 in y)
                {
                    RationalNumber coef = item.Value * item2.Value;
                    if (coef == 0)
                        continue;

                    UnitMonomial mono = item.Key * item2.Key;
                    if (!poly.TryGetValue(mono, out RationalNumber val))
                        val = 0;
                    poly[mono] = val + coef;
                }
            }
            poly.Trim();
            return poly;
        }

        public static Polynomial operator *(Polynomial x, UnitMonomial y)
        {
            if (x.Context != y.Context)
                throw new DifferentContextException();

            var poly = new Polynomial(x.Context);
            foreach (var item in x)
            {
                poly[item.Key * y] = item.Value;
            }
            return poly;
        }

        public static Polynomial operator /(Polynomial x, RationalNumber y)
        {
            if (y.IsZero)
                throw new DivideByZeroException();
            var poly = new Polynomial(x);
            foreach (var key in x.Keys)
            {
                poly[key] /= y;
            }
            return poly;
        }

        public static Polynomial operator /(Polynomial x, UnitMonomial y)
        {
            if (x.Context != y.Context)
                throw new DifferentContextException();

            var poly = new Polynomial(x.Context);
            foreach (var item in x)
            {
                poly[item.Key / y] = item.Value;
            }
            return poly;
        }

        public Polynomial Power(BigInteger pow)
        {
            var poly = new Polynomial(this);
            var ans = new Polynomial(1, Context);
            while (pow > 0)
            {
                if (pow % 2 == 1)
                {
                    ans *= poly;
                }
                pow /= 2;
                poly *= poly;
            }
            return ans;
        }
    }
}
