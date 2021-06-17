using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AlgebraicExpressionSimplifier
{
    public class Symbol : IExpressionContextHolder<Symbol>, IComparable<Symbol>, IEquatable<Symbol>
    {
        public string Name { get; }
        public ExpressionContext Context { get; }
        public IExpression Constraint
        {
            get => Context.GetSymbolConstraint(this);
        }

        public Symbol(string name, ExpressionContext context)
        {
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
            return other != null ? Name.CompareTo(other.Name) : 1;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Symbol);
        }

        public bool Equals(Symbol other)
        {
            return other != null &&
                   Name == other.Name &&
                   EqualityComparer<ExpressionContext>.Default.Equals(Context, other.Context);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Context);
        }

        public static bool operator ==(Symbol left, Symbol right)
        {
            return EqualityComparer<Symbol>.Default.Equals(left, right);
        }

        public static bool operator !=(Symbol left, Symbol right)
        {
            return !(left == right);
        }
    }
}
