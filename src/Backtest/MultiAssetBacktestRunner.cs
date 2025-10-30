using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuantFrameworks.Feeds;
using QuantFrameworks.Execution;
using QuantFrameworks.Portfolio;
using QuantFrameworks.Reporting;
using QuantFrameworks.Strategy;
using QuantFrameworks.Costs;
using QuantFrameworks.Sizing;


namespace QuantFrameworks.Backtest
{
    public sealed class MultiAssetBacktestRunner
    {
        private readonly BacktestConfig _cfg;
        public MultiAssetBacktestRunner(BacktestConfig cfg) => _cfg = cfg;

        public Task<(SummaryReport summary, RunReport run)> RunAsync(CancellationToken ct = default)
        {
            var symbols = _cfg.Symbols.Count > 0 ? _cfg.Symbols : new List<string> { _cfg.Symbol };
            if (symbols.Count == 0) throw new InvalidOperationException("No symbols configured.");

            IEnumerable<Bar> merged;
            if (_cfg.SymbolData.Count > 0)
            {
                var feed = new MultiCsvMarketDataFeed(new Dictionary<string, string>(_cfg.SymbolData, StringComparer.OrdinalIgnoreCase));
                merged = feed.ReadMerged(_cfg.Start, _cfg.End);
            }
            else
            {
                var single = new CsvMarketDataFeed(_cfg.DataPath);
                merged = single.ReadAsync(_cfg.Symbol, _cfg.Start, _cfg.End, ct).ToEnumerable();
            }

            var strat = new SmaCrossMultiStrategy(symbols, _cfg.Fast, _cfg.Slow, _cfg.StopLossPct, _cfg.TakeProfitPct);
            var broker = new SimpleBrokerSimulator();
            var pf = new PortfolioState(_cfg.StartingCash);
            ITransactionCostModel fees = new FixedAndPercentCostModel(_cfg.CommissionPerOrder, _cfg.PercentFee, _cfg.MinFee);

            var dailyNav = new List<(DateTime d, decimal nav)>();
            DateTime? lastDate = null;
            var lastPrices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            decimal lastCloseFor(string sym) => lastPrices.TryGetValue(sym, out var p) ? p : 0m;

            Bar? prevBar = null;
            var ordersQueue = new List<Order>();

            foreach (var bar in merged)
            {
                if (lastDate.HasValue && bar.Date.Date > lastDate.Value.Date)
                {
                    var nav = pf.Cash;
                    foreach (var kv in pf.Positions)
                        nav += lastCloseFor(kv.Key) * kv.Value.Quantity;
                    dailyNav.Add((lastDate.Value.Date, nav));
                }
                lastDate = bar.Date.Date;

                ordersQueue.AddRange(strat.OnBar(bar));

                if (prevBar is not null)
                {
                    foreach (var fill in broker.Match(ordersQueue, bar, bar.Date))
                    {
                        var slipped = SimpleSlippage.Apply(bar.Open, fill.Quantity, _cfg.SlippageBps);
                        var pos = pf.GetOrCreate(fill.Symbol);
                        var tradeCash = -(slipped * fill.Quantity);
                        var fee = fees.Compute(slipped, fill.Quantity, fill.Symbol);
                        pf.ApplyCash(tradeCash - fee);
                        pos.ApplyFill(fill.Quantity, slipped);
                    }
                    ordersQueue.Clear();
                }
                prevBar = bar;

                lastPrices[bar.Symbol] = bar.Close;
            }

            if (lastDate.HasValue)
            {
                var nav = pf.Cash;
                foreach (var kv in pf.Positions)
                    nav += lastCloseFor(kv.Key) * kv.Value.Quantity;
                dailyNav.Add((lastDate.Value.Date, nav));
            }

            var snap = new IO.PortfolioSnapshot { Cash = pf.Cash };
            foreach (var kv in pf.Positions)
            {
                snap.Positions[kv.Key] = new IO.PortfolioSnapshot.Position
                {
                    Symbol = kv.Key,
                    Quantity = kv.Value.Quantity,
                    AvgPrice = kv.Value.AvgPrice,
                    Currency = "USD"
                };
            }
            var summary = SummaryReporter.FromSnapshot(snap, lastPrices);

            var run = new RunReport
            {
                Start = _cfg.Start,
                End = _cfg.End,
                StartingCash = _cfg.StartingCash,
                EndingNAV = dailyNav.Count > 0 ? dailyNav[^1].nav : summary.NAV
            };
            foreach (var (d, n) in dailyNav)
                run.DailyNav.Add((d, n));

            var wealth = PerformanceSeries.WealthFromNAV(dailyNav.Select(x => x.nav).ToList());
            var (mdd, _, _) = PerformanceSeries.MaxDrawdown(wealth);
            run.MaxDrawdown = mdd;

            return Task.FromResult((summary, run));
        }
    }

    internal static class AsyncEnumExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this IAsyncEnumerable<T> source)
        {
            var e = source.GetAsyncEnumerator();
            try
            {
                while (true)
                {
                    var move = e.MoveNextAsync();
                    move.AsTask().Wait();
                    if (!move.Result) break;
                    yield return e.Current;
                }
            }
            finally { e.DisposeAsync().AsTask().Wait(); }
        }
    }
}
