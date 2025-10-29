using System;

namespace QuantFrameworks.Backtest
{
    public sealed class BacktestConfig
    {
        public string DataPath { get; init; } = "examples/data/prices.csv";
        public string Symbol { get; init; } = "AAPL";
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public decimal StartingCash { get; init; } = 100_000m;
        public int Fast { get; init; } = 5;
        public int Slow { get; init; } = 20;
        public string OutputPath { get; init; } = "out/summary.csv";
    }
}