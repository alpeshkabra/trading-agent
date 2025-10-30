using System;

namespace QuantFrameworks.Sizing
{
    public static class SizerFactory
    {
        public static IPositionSizer FromConfig(
            string? mode,
            decimal dollarsPerTrade,
            decimal percentNavPerTrade,
            int lotSize)
        {
            var lots = Math.Max(1, lotSize);
            var m = (mode ?? "").Trim().ToLowerInvariant();
            return m switch
            {
                "percentnav" or "percent" => new PercentNavSizer(percentNavPerTrade, lots),
                "fixeddollar" or "fixed" or "" => new FixedDollarSizer(dollarsPerTrade, lots),
                _ => throw new ArgumentException($"Unknown sizing mode: {mode}")
            };
        }
    }
}
