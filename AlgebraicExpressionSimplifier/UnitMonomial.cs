using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace MathematicalExpressionCalculator
{
    public class UnitMonomial : Dictionary<Symbol, RationalNumber>, IEquatable<UnitMonomial>, IExpressionContextHolder<UnitMonomial>, IComparable<UnitMonomial>
    {
        public ExpressionContext Context { get; }

        public UnitMonomial(ExpressionContext context)
        {
            Context = context;
        }

        public UnitMonomial(UnitMonomial mono) : base(mono)
        {
            Context = mono.Context;
        }

        public UnitMonomial(Symbol sy, RationalNumber power)
        {
            Context = sy.Context;
            this[sy] = power;
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

        public UnitMonomial WithContext(ExpressionContext context)
        {
            var mono = new UnitMonomial(context);
            foreach (var item in this)
            {
                mono[item.Key.WithContext(context)] = item.Value;
            }
            return mono;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UnitMonomial);
        }

        public bool Equals(UnitMonomial other)
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

        public static bool operator ==(UnitMonomial left, UnitMonomial right)
        {
            return EqualityComparer<UnitMonomial>.Default.Equals(left, right);
        }

        public static bool operator !=(UnitMonomial left, UnitMonomial right)
        {
            return !(left == right);
        }

        public static UnitMonomial operator *(UnitMonomial x, UnitMonomial y)
        {
            if (x.Context != y.Context)
                throw new DifferentContextException();

            var mono = new UnitMonomial(x);
            foreach (var item in y)
            {
                if (!mono.TryGetValue(item.Key, out RationalNumber val))
                    val = 0;
                mono[item.Key] = val + item.Value;
            }
            mono.Trim();
            return mono;
        }
        public static UnitMonomial operator /(UnitMonomial x, UnitMonomial y)
        {
            if (x.Context != y.Context)
                throw new DifferentContextException();

            var mono = new UnitMonomial(x);
            foreach (var item in y)
            {
                if (!mono.TryGetValue(item.Key, out RationalNumber val))
                    val = 0;
                mono[item.Key] = val - item.Value;
            }
            mono.Trim();
            return mono;
        }

        public UnitMonomial Power(RationalNumber pow)
        {
            var mono = new UnitMonomial(this);
            foreach (var key in mono.Keys.ToArray())
            {
                mono[key] *= pow;
            }
            return mono;
        }

        public UnitMonomial Reciprocal()
        {
            var mono = new UnitMonomial(this);
            foreach(var key in Keys)
            {
                mono[key] = -mono[key];
            }
            return mono;
        }

        public UnitMonomial Intersect(UnitMonomial other)
        {
            if (Context != other.Context)
                throw new DifferentContextException();

            UnitMonomial mono = new UnitMonomial(Context);
            foreach (var item in this)
            {
                if (other.TryGetValue(item.Key, out var pow))
                    mono[item.Key] = pow < item.Value ? pow : item.Value;
            }
            return mono;
        }
        public UnitMonomial Join(UnitMonomial other)
        {
            if (Context != other.Context)
                throw new DifferentContextException();

            UnitMonomial mono = new UnitMonomial(Context);
            foreach (var item in this)
            {
                if (other.TryGetValue(item.Key, out var pow))
                    mono[item.Key] = pow > item.Value ? pow : item.Value;
            }
            return mono;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var item in this)
            {
                sb.Append(item.Key.ToString());
                if (item.Value != 1)
                {
                    sb.Append("^");
                    sb.Append(item.Value);
                }
            }
            return sb.ToString();
        }
        public bool IsSymbol => Count == 1 && this.Single().Value.IsOne;
        public bool TryGetAsSymbol(out Symbol symbol)
        {
            if (Count != 1)
            {
                symbol = null;
                return false;
            }

            var item = this.Single();
            symbol = item.Key;
            return item.Value == 1;
        }

        public int CompareTo([AllowNull] UnitMonomial other)
        {
            if (other == null)
                return 1;
            var join = Join(other);
            foreach (var sy in join.Keys)
            {
                RationalNumber p = 0, q = 0;
                TryGetValue(sy, out p);
                other.TryGetValue(sy, out q);
                if (p != q)
                    return p.CompareTo(q);
            }
            return 0;
        }
    }
}
