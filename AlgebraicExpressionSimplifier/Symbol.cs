using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AlgebraicExpressionSimplifier
{
    public struct Symbol : IExpressionContextHolder, IComparable<Symbol>, IEquatable<Symbol>
    {
        public int ID { get; }
        public string Name { get; }
        public ExpressionContext Context { get; }

        internal Symbol(int id, string name, ExpressionContext context)
        {
            ID = id;
            Name = name;
            Context = context;
        }

        public Symbol WithContext(ExpressionContext context)
        {
            return context.Symbol(Name);
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo([AllowNull] Symbol other)
        {
            if (other != null)
                return ID.CompareTo(other.ID);
            else
                return 1;
        }

        public override bool Equals(object obj)
        {
            return obj is Symbol symbol && Equals(symbol);
        }

        public bool Equals(Symbol other)
        {
            return ID == other.ID &&
                   Name == other.Name &&
                   EqualityComparer<ExpressionContext>.Default.Equals(Context, other.Context);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Name, Context);
        }

        public static bool operator ==(Symbol left, Symbol right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Symbol left, Symbol right)
        {
            return !(left == right);
        }
    }
}
