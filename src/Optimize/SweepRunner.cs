using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuantFrameworks.Optimize
{
    public sealed class SweepRunner
    {
        private readonly OptimizerConfig _cfg;
        public SweepRunner(OptimizerConfig cfg) => _cfg = cfg;

        public SweepResult Run()
        {
            var grid = GridGenerator.Cartesian(_cfg.Parameters).ToList();
            var bag = new ConcurrentBag<RunResult>();

            Parallel.ForEach(
                grid,
                new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, _cfg.MaxDegreeOfParallelism) },
                ps =>
                {
                    var bc = ps.Apply(CloneBase(_cfg.BaseBacktest));
                    var runner = new Backtest.MultiAssetBacktestRunner(bc);
                    var (summary, run) = runner.RunAsync().GetAwaiter().GetResult();

                    // Compute total return from NAV vs StartingCash (SummaryReport may not have it)
                    decimal totalReturn = bc.StartingCash != 0m
                        ? (summary.NAV / bc.StartingCash) - 1m
                        : 0m;

                    bag.Add(new RunResult
                    {
                        Params = ps,
                        NAV = summary.NAV,
                        Sharpe = summary.Sharpe,
                        TotalReturn = totalReturn,
                        Label = ps.ToString()
                    });
                });

            var res = new SweepResult { Metric = _cfg.TargetMetric };
            foreach (var r in bag) res.Runs.Add(r);
            Directory.CreateDirectory(_cfg.OutputDir);
            return res;
        }

        private static Backtest.BacktestConfig CloneBase(Backtest.BacktestConfig b)
        {
            return new Backtest.BacktestConfig
            {
                Symbol = b.Symbol,
                DataPath = b.DataPath,
                Symbols = new List<string>(b.Symbols),
                SymbolData = new Dictionary<string, string>(b.SymbolData, StringComparer.OrdinalIgnoreCase),
                Start = b.Start,
                End = b.End,
                StartingCash = b.StartingCash,
                Fast = b.Fast,
                Slow = b.Slow,
                StopLossPct = b.StopLossPct,
                TakeProfitPct = b.TakeProfitPct,
                CommissionPerOrder = b.CommissionPerOrder,
                PercentFee = b.PercentFee,
                MinFee = b.MinFee,
                SlippageBps = b.SlippageBps,
                OutputPath = b.OutputPath,
                DailyNavCsv = b.DailyNavCsv,
                RunJson = b.RunJson,
                SizingMode = b.SizingMode,
                DollarsPerTrade = b.DollarsPerTrade,
                PercentNavPerTrade = b.PercentNavPerTrade,
                LotSize = b.LotSize,
                MaxGrossExposurePct = b.MaxGrossExposurePct
            };
        }
    }
}
