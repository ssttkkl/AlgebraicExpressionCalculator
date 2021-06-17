using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;

namespace AlgebraicExpressionSimplifier
{
    public class Polynomial : Dictionary<UnitMonomial, RationalNumber>, IExpression, IEquatable<Polynomial>
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

        public Polynomial(UnitMonomial mono, RationalNumber coef)
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

        public override bool Equals(object obj)
        {
            return Equals(obj as UnitMonomial);
        }

        public bool Equals(Polynomial other)
        {
            if (other == null)
                return false;

            foreach (var item in this)
            {
                if (other[item.Key] != item.Value)
                    return false;
            }
            foreach (var item in other)
            {
                if (this[item.Key] != item.Value)
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var item in this)
            {
                hash.Add(item.Key);
                hash.Add(item.Value);
            }
            return hash.ToHashCode();
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
            var sb = new StringBuilder();
            foreach (var item in this)
            {
                if (sb.Length > 0 && item.Value > 0)
                    sb.Append("+");
                if (item.Key.Count == 0)
                    sb.Append(item.Value);
                else if (item.Value == -1)
                    sb.Append("-");
                else if (item.Value != 1)
                    sb.Append(item.Value);
                foreach (var item2 in item.Key)
                {
                    sb.Append(item2.Key.ToString());
                    if (item2.Value != 1)
                    {
                        sb.Append("^");
                        sb.Append(item2.Value);
                    }
                }
            }
            return sb.ToString();
        }

        public bool TryGetAsNumber(out RationalNumber number)
        {
            if (Count != 1)
            {
                number = 0;
                return false;
            }

            var item = this.Single();
            number = item.Value;
            return item.Key.Count == 0;
        }

        public bool TryGetAsSymbol(out Symbol symbol)
        {
            if (Count != 1)
            {
                symbol = new Symbol();
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
