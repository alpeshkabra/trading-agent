using System;
using System.Collections.Generic;
using System.Linq;
using QuantFrameworks.Models;
using QuantFrameworks.Risk;

namespace QuantFrameworks.Risk.Sizing
{
    public interface IPositionSizer
    {
        int Size(in Order order);
    }

    public static class PositionSizerFactory
    {
        public static IPositionSizer Create(RiskConfig cfg, Dictionary<(DateOnly,string),decimal> prices)
        {
            return cfg.Sizing.Mode?.ToLowerInvariant() switch
            {
                "fixedfraction" => new FixedFractionSizer(cfg),
                "voltarget"     => new VolTargetSizer(cfg, prices),
                _               => new PassthroughSizer()
            };
        }
    }

    internal sealed class PassthroughSizer : IPositionSizer
    {
        public int Size(in Order order) => order.Qty; // use incoming qty
    }

    public sealed class FixedFractionSizer : IPositionSizer
    {
        private readonly RiskConfig _cfg;
        public FixedFractionSizer(RiskConfig cfg) { _cfg = cfg; }

        public int Size(in Order order)
        {
            var budget = _cfg.Sizing.Capital * (decimal)_cfg.Sizing.FixedFraction;
            var qty = (int)Math.Floor(budget / Math.Max(1m, order.Price));
            return order.Side.Equals("SELL", StringComparison.OrdinalIgnoreCase) ? -qty : qty;
        }
    }

    public sealed class VolTargetSizer : IPositionSizer
    {
        private readonly RiskConfig _cfg;
        private readonly Dictionary<(DateOnly,string),decimal> _prices;

        public VolTargetSizer(RiskConfig cfg, Dictionary<(DateOnly,string),decimal> prices)
        {
            _cfg = cfg; _prices = prices;
        }

        public int Size(in Order order)
        {
            // copy off 'order' fields so lambdas don't capture the 'in' parameter
            var symbol = order.Symbol;
            var side   = order.Side;
            var price  = order.Price;

            // estimate daily vol from close-to-close returns over lookback
            var lookback = Math.Max(2, _cfg.Sizing.LookbackDays);

            // tuple keys are (DateOnly date, string symbol) => use Item1/Item2 (or deconstruction)
            var keyDates = _prices.Keys
                                  .Where(k => string.Equals(k.Item2, symbol, StringComparison.OrdinalIgnoreCase))
                                  .Select(k => k.Item1)
                                  .Distinct()
                                  .OrderBy(d => d)
                                  .ToArray();

            if (keyDates.Length < lookback) return 0;

            var startIdx = Math.Max(0, keyDates.Length - lookback);
            var window = keyDates[startIdx..];

            var closes = window.Select(d => (double)_prices[(d, symbol)]).ToArray();
            var rets = new double[closes.Length - 1];
            for (int i = 1; i < closes.Length; i++)
                rets[i - 1] = (closes[i] - closes[i - 1]) / closes[i - 1];

            var mean = rets.Average();
            var var_ = rets.Length > 1 ? rets.Select(r => (r - mean) * (r - mean)).Sum() / (rets.Length - 1) : 0.0;
            var dailyVol = Math.Sqrt(var_);
            if (dailyVol <= 0) return 0;

            const double TRADING_DAYS = 252.0;
            var targetDaily = _cfg.Sizing.VolTargetAnnual / Math.Sqrt(TRADING_DAYS);

            // qty = (capital * target_vol / (price * daily_vol))
            var qtyF = (double)_cfg.Sizing.Capital * targetDaily / ((double)Math.Max(1m, price) * dailyVol);
            var qty = (int)Math.Floor(Math.Max(0, qtyF));
            return side.Equals("SELL", StringComparison.OrdinalIgnoreCase) ? -qty : qty;
        }
    }
}
