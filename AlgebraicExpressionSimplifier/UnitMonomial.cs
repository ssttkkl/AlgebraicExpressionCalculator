using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MathematicalExpressionCalculator
{
    public class UnitMonomial : SortedDictionary<Symbol, RationalNumber>, IEquatable<UnitMonomial>, IExpressionContextHolder<UnitMonomial>
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

        public static bool operator ==(UnitMonomial left, UnitMonomial right)
        {
            return EqualityComparer<UnitMonomial>.Default.Equals(left, right); ;
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

        public UnitMonomial Inv()
        {
            var mono = new UnitMonomial(this);
            foreach(var key in Keys)
            {
                mono[key] = -mono[key];
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
    }
}
