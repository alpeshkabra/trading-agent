using System;
using System.Collections.Generic;

namespace QuantFrameworks.Backtest
{
    public sealed class BacktestConfig
    {
        public string Symbol { get; init; } = "";
        public string DataPath { get; init; } = "";
        public List<string> Symbols { get; init; } = new();
        public Dictionary<string, string> SymbolData { get; init; } = new();

        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public decimal StartingCash { get; init; } = 100_000m;

        public int Fast { get; init; } = 5;
        public int Slow { get; init; } = 20;
        public decimal StopLossPct { get; init; } = 0m;
        public decimal TakeProfitPct { get; init; } = 0m;

        // Costs & slippage
        public decimal CommissionPerOrder { get; init; } = 0m;
        public decimal PercentFee { get; init; } = 0m;
        public decimal MinFee { get; init; } = 0m;
        public decimal SlippageBps { get; init; } = 0m;

        // Outputs
        public string OutputPath { get; init; } = "out/summary.csv";
        public string DailyNavCsv { get; init; } = "out/daily_nav.csv";
        public string RunJson { get; init; } = "out/run.json";

        public string? SizingMode { get; init; } = "FixedDollar";
        public decimal DollarsPerTrade { get; init; } = 10_000m;
        public decimal PercentNavPerTrade { get; init; } = 0m;
        public int LotSize { get; init; } = 1;
        public decimal MaxGrossExposurePct { get; init; } = 0m;

    }
}
