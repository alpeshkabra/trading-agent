using System;
using System.Collections.Generic;

namespace QuantFrameworks.Portfolio
{
    public sealed class Position
    {
        public string Symbol { get; }
        public int Quantity { get; private set; }
        public decimal AvgPrice { get; private set; }

        public Position(string symbol, int qty = 0, decimal avg = 0m)
        {
            Symbol = symbol;
            Quantity = qty;
            AvgPrice = avg;
        }

        public void ApplyFill(int deltaQty, decimal fillPrice)
        {
            if (deltaQty == 0) return;
            var newQty = Quantity + deltaQty;

            if (Math.Sign(Quantity) == Math.Sign(newQty) || Quantity == 0)
            {
                var notionalOld = AvgPrice * Quantity;
                var notionalNew = fillPrice * deltaQty;
                var totalQty = Quantity + deltaQty;
                AvgPrice = totalQty == 0 ? 0m : (notionalOld + notionalNew) / totalQty;
            }
            else
            {
                if (Math.Sign(Quantity) != Math.Sign(newQty))
                    AvgPrice = fillPrice;
            }

            Quantity = newQty;
        }
    }

    public sealed class PortfolioState
    {
        public decimal Cash { get; private set; }
        private readonly Dictionary<string, Position> _positions = new(StringComparer.OrdinalIgnoreCase);

        public PortfolioState(decimal startingCash) => Cash = startingCash;

        public IReadOnlyDictionary<string, Position> Positions => _positions;

        public Position GetOrCreate(string symbol)
        {
            if (!_positions.TryGetValue(symbol, out var pos))
                _positions[symbol] = pos = new Position(symbol);
            return pos;
        }

        public void ApplyCash(decimal delta) => Cash += delta;
    }
}