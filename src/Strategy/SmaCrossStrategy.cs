using System.Collections.Generic;
using System.Linq;
using QuantFrameworks.Feeds;
using QuantFrameworks.Execution;

namespace QuantFrameworks.Strategy
{
    public sealed class SmaCrossStrategy : IStrategy
    {
        private readonly string _symbol;
        private readonly int _fast;
        private readonly int _slow;
        private readonly Queue<decimal> _fastWin = new();
        private readonly Queue<decimal> _slowWin = new();
        private bool _hasPos;

        public SmaCrossStrategy(string symbol, int fast = 5, int slow = 20)
        {
            _symbol = symbol;
            _fast = fast;
            _slow = slow;
        }

        public IEnumerable<Order> OnBar(Bar bar)
        {
            if (!string.Equals(bar.Symbol, _symbol, System.StringComparison.OrdinalIgnoreCase))
                yield break;

            _fastWin.Enqueue(bar.Close);
            if (_fastWin.Count > _fast) _fastWin.Dequeue();

            _slowWin.Enqueue(bar.Close);
            if (_slowWin.Count > _slow) _slowWin.Dequeue();

            if (_fastWin.Count == _fast && _slowWin.Count == _slow)
            {
                var fastSma = _fastWin.Average();
                var slowSma = _slowWin.Average();

                if (!_hasPos && fastSma > slowSma)
                {
                    _hasPos = true;
                    yield return new Order(_symbol, +100, OrderType.Market, Tag: "BUY");
                }
                else if (_hasPos && fastSma < slowSma)
                {
                    _hasPos = false;
                    yield return new Order(_symbol, -100, OrderType.Market, Tag: "SELL");
                }
            }
        }
    }
}