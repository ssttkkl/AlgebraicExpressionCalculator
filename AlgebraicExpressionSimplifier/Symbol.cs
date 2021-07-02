using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MathematicalExpressionCalculator
{
    public class Symbol : IExpressionContextHolder<Symbol>, IComparable<Symbol>
    {
        public string Name { get; }
        public ExpressionContext Context { get; }
        public IExpression Assignment
        {
            get => Context.GetSymbolAssignment(this);
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
            return Name.Length == 1 ? Name : "{" + Name + "}";
        }

        public int CompareTo([AllowNull] Symbol other)
        {
            return other != null ? Name.CompareTo(other.Name) : 1;
        }
    }
}
