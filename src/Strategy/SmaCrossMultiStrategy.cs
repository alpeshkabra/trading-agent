using System.Collections.Generic;
using QuantFrameworks.Feeds;
using QuantFrameworks.Execution;

namespace QuantFrameworks.Strategy
{
    public sealed class SmaCrossMultiStrategy : IStrategy
    {
        private readonly Dictionary<string, SmaCrossStrategy> _bySymbol = new(System.StringComparer.OrdinalIgnoreCase);

        public SmaCrossMultiStrategy(IEnumerable<string> symbols, int fast, int slow, decimal stopLossPct = 0m, decimal takeProfitPct = 0m)
        {
            foreach (var s in symbols)
                _bySymbol[s] = new SmaCrossStrategy(s, fast, slow, stopLossPct, takeProfitPct);
        }

        public IEnumerable<Order> OnBar(Bar bar)
        {
            if (_bySymbol.TryGetValue(bar.Symbol, out var strat))
                return strat.OnBar(bar);
            return System.Array.Empty<Order>();
        }
    }
}
