using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantFrameworks.Optimize
{
    public sealed class ParamSet
    {
        public Dictionary<string, int> Values { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Backtest.BacktestConfig Apply(Backtest.BacktestConfig baseCfg)
        {
            // Determine overrides (fall back to base config values)
            int fast = Values.TryGetValue("Fast", out var f) ? f : baseCfg.Fast;
            int slow = Values.TryGetValue("Slow", out var s) ? s : baseCfg.Slow;
            int lot  = Values.TryGetValue("LotSize", out var l) ? l : baseCfg.LotSize;

            // Init-only properties must be set via object initializer
            return new Backtest.BacktestConfig
            {
                Symbol = baseCfg.Symbol,
                DataPath = baseCfg.DataPath,
                Symbols = new List<string>(baseCfg.Symbols),
                SymbolData = new Dictionary<string, string>(baseCfg.SymbolData, StringComparer.OrdinalIgnoreCase),
                Start = baseCfg.Start,
                End = baseCfg.End,
                StartingCash = baseCfg.StartingCash,

                // overrides
                Fast = fast,
                Slow = slow,
                LotSize = lot,

                // pass-throughs
                StopLossPct = baseCfg.StopLossPct,
                TakeProfitPct = baseCfg.TakeProfitPct,
                CommissionPerOrder = baseCfg.CommissionPerOrder,
                PercentFee = baseCfg.PercentFee,
                MinFee = baseCfg.MinFee,
                SlippageBps = baseCfg.SlippageBps,
                OutputPath = baseCfg.OutputPath,
                DailyNavCsv = baseCfg.DailyNavCsv,
                RunJson = baseCfg.RunJson,
                SizingMode = baseCfg.SizingMode,
                DollarsPerTrade = baseCfg.DollarsPerTrade,
                PercentNavPerTrade = baseCfg.PercentNavPerTrade,
                MaxGrossExposurePct = baseCfg.MaxGrossExposurePct
            };
        }

        public override string ToString()
            => string.Join(",", Values.Select(kv => $"{kv.Key}={kv.Value}"));
    }
}
