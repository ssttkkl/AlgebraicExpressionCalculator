using System;
using System.Collections.Generic;
using System.Text;

namespace AlgebraicExpressionSimplifier
{
    public class ExpressionContext
    {
        private readonly List<Symbol> symbols = new List<Symbol>();

        public IReadOnlyList<Symbol> Symbols { get => symbols; }

        public Symbol Symbol(string name)
        {
            foreach (var item in Symbols)
            {
                if (item.Name == name)
                {
                    return item;
                }
            }
            Symbol sy = new Symbol(symbols.Count, name, this);
            symbols.Add(sy);
            return sy;
        }
    }
    public interface IExpressionContextHolder
    {
        public ExpressionContext Context { get; }
    }

    public class DifferentContextException : ArgumentException { }
}
