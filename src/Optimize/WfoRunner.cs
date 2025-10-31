using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantFrameworks.Optimize
{
    public sealed class WfoRunner
    {
        private readonly OptimizerConfig _cfg;
        public WfoRunner(OptimizerConfig cfg) => _cfg = cfg;

        public WfoResult Run()
        {
            var wfo = _cfg.Wfo;
            if (wfo is null || wfo.KFolds <= 0) return new WfoResult();

            var folds = Splitter.KFoldWalkForward(
                _cfg.BaseBacktest.Start, _cfg.BaseBacktest.End, wfo.KFolds, wfo.TrainRatio);

            var res = new WfoResult { Metric = _cfg.TargetMetric };

            foreach (var (ts, te, vs, ve) in folds)
            {
                // Train: evaluate grid on [ts, te], choose best by metric
                var best = EvaluateGrid(ts, te).OrderByDescending(ByMetric(_cfg.TargetMetric)).FirstOrDefault();
                if (best is null) continue;

                // Test: run best params on validation window [vs, ve]
                var bcTest = CloneWithWindow(_cfg.BaseBacktest, best.Params, vs, ve);
                var runner = new Backtest.MultiAssetBacktestRunner(bcTest);
                var (summary, _) = runner.RunAsync().GetAwaiter().GetResult();

                res.Folds.Add(new WfoFoldResult
                {
                    TrainStart = ts, TrainEnd = te,
                    TestStart = vs, TestEnd = ve,
                    BestParams = best.Params,
                    TestNAV = summary.NAV,
                    TestSharpe = summary.Sharpe
                });
            }

            return res;
        }

        private IEnumerable<RunResult> EvaluateGrid(DateTime s, DateTime e)
        {
            var grid = GridGenerator.Cartesian(_cfg.Parameters).ToList();
            foreach (var ps in grid)
            {
                var bc = CloneWithWindow(_cfg.BaseBacktest, ps, s, e);
                var runner = new Backtest.MultiAssetBacktestRunner(bc);
                var (summary, _) = runner.RunAsync().GetAwaiter().GetResult();

                decimal totalReturn = bc.StartingCash != 0m
                    ? (summary.NAV / bc.StartingCash) - 1m
                    : 0m;

                yield return new RunResult
                {
                    Params = ps,
                    NAV = summary.NAV,
                    Sharpe = summary.Sharpe,
                    TotalReturn = totalReturn,
                    Label = ps.ToString()
                };
            }
        }

        private static Func<RunResult, decimal> ByMetric(string metric) => (metric ?? "Sharpe").ToLowerInvariant() switch
        {
            "nav"         => r => r.NAV,
            "totalreturn" => r => r.TotalReturn,
            _             => r => r.Sharpe
        };

        private static Backtest.BacktestConfig CloneWithWindow(Backtest.BacktestConfig baseCfg, ParamSet ps, DateTime start, DateTime end)
        {
            var bc = ps.Apply(baseCfg);
            // must set Start/End inside initializer -> rebuild with same fields, swapping window
            return new Backtest.BacktestConfig
            {
                Symbol = bc.Symbol,
                DataPath = bc.DataPath,
                Symbols = new List<string>(bc.Symbols),
                SymbolData = new Dictionary<string, string>(bc.SymbolData, StringComparer.OrdinalIgnoreCase),
                Start = start,
                End = end,
                StartingCash = bc.StartingCash,

                Fast = bc.Fast,
                Slow = bc.Slow,
                LotSize = bc.LotSize,
                StopLossPct = bc.StopLossPct,
                TakeProfitPct = bc.TakeProfitPct,
                CommissionPerOrder = bc.CommissionPerOrder,
                PercentFee = bc.PercentFee,
                MinFee = bc.MinFee,
                SlippageBps = bc.SlippageBps,
                OutputPath = bc.OutputPath,
                DailyNavCsv = bc.DailyNavCsv,
                RunJson = bc.RunJson,
                SizingMode = bc.SizingMode,
                DollarsPerTrade = bc.DollarsPerTrade,
                PercentNavPerTrade = bc.PercentNavPerTrade,
                MaxGrossExposurePct = bc.MaxGrossExposurePct
            };
        }
    }

    public sealed class WfoResult
    {
        public string Metric { get; init; } = "Sharpe";
        public List<WfoFoldResult> Folds { get; } = new();
    }

    public sealed class WfoFoldResult
    {
        public DateTime TrainStart { get; init; }
        public DateTime TrainEnd   { get; init; }
        public DateTime TestStart  { get; init; }
        public DateTime TestEnd    { get; init; }
        public ParamSet BestParams { get; init; } = new();
        public decimal TestNAV { get; init; }
        public decimal TestSharpe { get; init; }
    }
}
