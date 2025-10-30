using System;

namespace QuantFrameworks.Sizing
{
    public sealed class PercentNavSizer : IPositionSizer
    {
        public decimal PercentOfNav { get; } // e.g. 0.05 = 5% NAV per trade
        public int LotSize { get; }

        public PercentNavSizer(decimal percentOfNav, int lotSize = 1)
        {
            PercentOfNav = percentOfNav < 0 ? 0 : percentOfNav;
            LotSize = Math.Max(1, lotSize);
        }

        public int Size(decimal price, decimal nav)
        {
            if (price <= 0 || nav <= 0 || PercentOfNav <= 0) return 0;
            var dollars = nav * PercentOfNav;
            var raw = (int)Math.Floor(dollars / price);
            if (raw <= 0) return 0;
            return Math.Max(LotSize, (raw / LotSize) * LotSize);
        }
    }
}
