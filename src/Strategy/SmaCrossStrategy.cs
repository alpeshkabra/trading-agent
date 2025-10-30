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
        private readonly decimal _stopLossPct;   // 0.05 = 5%
        private readonly decimal _takeProfitPct; // 0.10 = 10%

        private readonly Queue<decimal> _fastWin = new();
        private readonly Queue<decimal> _slowWin = new();

        private bool _hasPos;
        private decimal _entryPrice;
        private int _positionQty = 100; // fixed size for MVP

        public SmaCrossStrategy(string symbol, int fast = 5, int slow = 20,
                                decimal stopLossPct = 0m, decimal takeProfitPct = 0m)
        {
            _symbol = symbol;
            _fast = fast;
            _slow = slow;
            _stopLossPct = stopLossPct < 0 ? 0 : stopLossPct;
            _takeProfitPct = takeProfitPct < 0 ? 0 : takeProfitPct;
        }

        public IEnumerable<Order> OnBar(Bar bar)
        {
            if (!string.Equals(bar.Symbol, _symbol, System.StringComparison.OrdinalIgnoreCase))
                yield break;

            // 1) Risk exits first (if in position)
            if (_hasPos && _entryPrice > 0m)
            {
                if (_stopLossPct > 0m)
                {
                    var stopLevel = _entryPrice * (1m - _stopLossPct);
                    if (bar.Low <= stopLevel)
                    {
                        _hasPos = false;
                        _entryPrice = 0m;
                        yield return new Order(_symbol, -_positionQty, OrderType.Market, null, Tag: "STOP_EXIT");
                        yield break; // exit takes precedence on this bar
                    }
                }

                if (_takeProfitPct > 0m)
                {
                    var tpLevel = _entryPrice * (1m + _takeProfitPct);
                    if (bar.High >= tpLevel)
                    {
                        _hasPos = false;
                        _entryPrice = 0m;
                        yield return new Order(_symbol, -_positionQty, OrderType.Market, null, Tag: "TP_EXIT");
                        yield break;
                    }
                }
            }

            // 2) SMA crossover logic
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
                    _entryPrice = bar.Close;
                    yield return new Order(_symbol, +_positionQty, OrderType.Market, null, Tag: "BUY");
                }
                else if (_hasPos && fastSma < slowSma)
                {
                    _hasPos = false;
                    _entryPrice = 0m;
                    yield return new Order(_symbol, -_positionQty, OrderType.Market, null, Tag: "SELL");
                }
            }
        }
    }
}
