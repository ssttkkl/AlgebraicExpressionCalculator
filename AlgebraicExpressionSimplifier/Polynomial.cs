using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AlgebraicExpressionSimplifier
{
    public class Polynomial : Dictionary<UnitMonomial, double>, IExpression, IEquatable<Polynomial>
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

        public Polynomial(double number, ExpressionContext context)
        {
            Context = context;
            this[new UnitMonomial(context)] = number;
        }

        public Polynomial(Symbol sy, double power = 1)
        {
            Context = sy.Context;
            this[new UnitMonomial(sy, power)] = 1;
        }

        public Polynomial(UnitMonomial mono, double coef = 1)
        {
            Context = mono.Context;
            this[mono] = coef;
        }

        public void Trim()
        {
            foreach (var key in Keys.ToArray())
            {
                if (TryGetValue(key, out double val) && val == 0)
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

        public bool TryGetAsNumber(out double number)
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
                symbol = null;
                return false;
            }

            var item = this.Single();
            return item.Key.TryGetAsSymbol(out symbol) && item.Value == 1;
        }

        public bool TryGetAsMonomial(out double coef, out UnitMonomial monomial)
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

        public Polynomial Plus(Polynomial o)
        {
            if (Context != o.Context)
                throw new DifferentContextException();

            var poly = new Polynomial(this);
            foreach (var item in o)
            {
                if (!poly.TryGetValue(item.Key, out double val))
                    val = 0;
                poly[item.Key] = val + item.Value;
            }
            poly.Trim();
            return poly;
        }

        public Polynomial Minus(Polynomial o)
        {
            if (Context != o.Context)
                throw new DifferentContextException();

            var poly = new Polynomial(this);
            foreach (var item in o)
            {
                if (!poly.TryGetValue(item.Key, out double val))
                    val = 0;
                poly[item.Key] = val - item.Value;
            }
            poly.Trim();
            return poly;
        }

        public Polynomial Times(Polynomial o)
        {
            if (Context != o.Context)
                throw new DifferentContextException();

            var poly = new Polynomial(Context);
            foreach (var item in this)
            {
                foreach (var item2 in o)
                {
                    double coef = item.Value * item2.Value;
                    if (coef == 0)
                        continue;

                    UnitMonomial mono = item.Key.Times(item2.Key);
                    if (!poly.TryGetValue(mono, out double val))
                        val = 0;
                    poly[mono] = val + coef;
                }
            }
            poly.Trim();
            return poly;
        }

        public Polynomial Divide(double num)
        {
            if (num == 0)
                throw new DivideByZeroException();
            var poly = new Polynomial(this);
            foreach (var key in Keys)
            {
                poly[key] /= num;
            }
            return poly;
        }

        public Polynomial Power(int pow)
        {
            var poly = new Polynomial(this);
            var ans = new Polynomial(1, Context);
            while (pow > 0)
            {
                if (pow % 2 == 1)
                {
                    ans = ans.Times(poly);
                }
                pow /= 2;
                poly = poly.Times(poly);
            }
            return ans;
        }
    }
}
