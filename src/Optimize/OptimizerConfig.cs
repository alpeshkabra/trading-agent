using System;
using System.Collections.Generic;

namespace QuantFrameworks.Optimize
{
    public sealed class OptimizerConfig
    {
        public Backtest.BacktestConfig BaseBacktest { get; init; } = new();
        public List<ParamSpec> Parameters { get; init; } = new();
        public string TargetMetric { get; init; } = "Sharpe"; // or NAV, TotalReturn
        public int MaxDegreeOfParallelism { get; init; } = Math.Max(1, Environment.ProcessorCount / 2);
        public int TopN { get; init; } = 10;
        public string OutputDir { get; init; } = "out/optimize";
        public WfoConfig? Wfo { get; init; }
    }

    public sealed class ParamSpec
    {
        public string Name { get; init; } = "";
        // one of:
        public int? From { get; init; }
        public int? To { get; init; }
        public int? Step { get; init; }
        public List<int>? Values { get; init; }
    }

    public sealed class WfoConfig
    {
        public int KFolds { get; init; } = 0;
        public double TrainRatio { get; init; } = 0.7; // fraction per fold
    }
}
