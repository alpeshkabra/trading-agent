using System;

namespace QuantFrameworks.Sizing
{
    public sealed class FixedDollarSizer : IPositionSizer
    {
        public decimal DollarsPerTrade { get; }
        public int LotSize { get; }

        public FixedDollarSizer(decimal dollarsPerTrade, int lotSize = 1)
        {
            DollarsPerTrade = Math.Max(0, dollarsPerTrade);
            LotSize = Math.Max(1, lotSize);
        }

        public int Size(decimal price, decimal nav)
        {
            if (price <= 0 || DollarsPerTrade <= 0) return 0;
            var raw = (int)Math.Floor(DollarsPerTrade / price);
            if (raw <= 0) return 0;
            return Math.Max(LotSize, (raw / LotSize) * LotSize);
        }
    }
}
