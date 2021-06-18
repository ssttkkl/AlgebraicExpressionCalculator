using System;
using System.Collections.Generic;

namespace MathematicalExpressionCalculator
{
    public class ExpressionContext
    {
        private readonly Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();
        private readonly Dictionary<Symbol, IExpression> symbolAssignment = new Dictionary<Symbol, IExpression>();

        public IReadOnlyCollection<Symbol> Symbols => symbols.Values;
        public Symbol Symbol(string name)
        {
            foreach (var item in Symbols)
            {
                if (item.Name == name)
                {
                    return item;
                }
            }
            var sy = new Symbol(name, this);
            symbols[name] = sy;
            return sy;
        }
        public IExpression GetSymbolAssignment(Symbol symbol)
        {
            if (symbol.Context != this)
                throw new DifferentContextException();
            else
            {
                if (symbolAssignment.TryGetValue(symbol, out var ep))
                    return ep;
                return null;
            }
        }
        public void SetSymbolAssignment(Symbol symbol, IExpression expression)
        {
            if (symbol.Context != this)
                throw new DifferentContextException();
            else
            {
                if (expression.Context != this)
                    expression = expression.WithContext(this);
                symbolAssignment[symbol] = expression;
            }
        }
        public ExpressionContext Clone()
        {
            var context = new ExpressionContext();
            foreach (var item in Symbols)
            {
                context.SetSymbolAssignment(item.WithContext(context), item.Assignment);
            }
            return context;
        }
        public void AnalyseConstraints()
        {
            var n = 0;
            var symbolId = new Dictionary<string, int>();
            var symbolList = new List<Symbol>();
            foreach (var item in symbols)
            {
                symbolId[item.Key] = n;
                symbolList.Add(item.Value);
                n++;
            }

            // 建图
            var graph = new List<int>[n];
            var indegree = new int[n];
            for (int i = 0; i < n; i++)
            {
                graph[i] = new List<int>();
            }

            foreach (var item in symbolAssignment)
            {
                var relation = new HashSet<Symbol>();
                AnalyseRelation(item.Value, relation);

                int from = symbolId[item.Key.Name];
                foreach (var sy in relation)
                {
                    int to = symbolId[sy.Name];
                    graph[from].Add(to);
                    indegree[to]++;
                }
            }

            // 拓扑排序
            var order = new Stack<int>();
            var que = new Queue<int>();
            for (int i = 0; i < n; i++)
            {
                if (indegree[i] == 0)
                {
                    que.Enqueue(i);
                }
            }
            while (que.Count > 0)
            {
                int v = que.Dequeue();
                order.Push(v);

                foreach (var w in graph[v])
                {
                    if (--indegree[w] == 0)
                    {
                        que.Enqueue(w);
                    }
                }
            }

            // 按照排序的逆序将变量赋值逐条代入并简化
            if (order.Count != n)
                throw new InvalidOperationException("变量存在循环引用");
            else
            {
                while (order.Count > 0)
                {
                    int v = order.Pop();
                    var sy = symbolList[v];
                    var ep = sy.Assignment;
                    if (ep != null)
                    {
                        ep = ep.WithAssignment().Simplify();
                        SetSymbolAssignment(sy, ep);
                    }
                }
            }
        }
        private static void AnalyseRelation(IExpression ep, HashSet<Symbol> relation)
        {
            if (ep is Polynomial poly)
            {
                foreach (var mono in poly.Keys)
                {
                    foreach (var sy in mono.Keys)
                    {
                        relation.Add(sy);
                    }
                }
            }
            else if (ep is ExpressionTree tree)
            {
                AnalyseRelation(tree.Left, relation);
                AnalyseRelation(tree.Right, relation);
            }
        }
    }
    public interface IExpressionContextHolder<out T> where T : IExpressionContextHolder<T>
    {
        public ExpressionContext Context { get; }
        public T WithContext(ExpressionContext context);
    }

    public class DifferentContextException : ArgumentException { }
}
