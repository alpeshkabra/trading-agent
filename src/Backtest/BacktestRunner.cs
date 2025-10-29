using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuantFrameworks.Feeds;
using QuantFrameworks.Execution;
using QuantFrameworks.Portfolio;
using QuantFrameworks.Reporting;
using QuantFrameworks.Strategy;

namespace QuantFrameworks.Backtest
{
    public sealed class BacktestRunner
    {
        private readonly BacktestConfig _cfg;
        public BacktestRunner(BacktestConfig cfg) => _cfg = cfg;

        public async Task<SummaryReport> RunAsync(CancellationToken ct = default)
        {
            var feed = new CsvMarketDataFeed(_cfg.DataPath);
            var strat = new SmaCrossStrategy(_cfg.Symbol, _cfg.Fast, _cfg.Slow);
            var broker = new SimpleBrokerSimulator();
            var pf = new PortfolioState(_cfg.StartingCash);

            Bar? prevBar = null;
            var ordersQueue = new List<Order>();

            await foreach (var bar in feed.ReadAsync(_cfg.Symbol, _cfg.Start, _cfg.End, ct))
            {
                ordersQueue.AddRange(strat.OnBar(bar));

                if (prevBar is not null)
                {
                    foreach (var fill in broker.Match(ordersQueue, bar, bar.Date))
                    {
                        var pos = pf.GetOrCreate(fill.Symbol);
                        pf.ApplyCash(-(fill.Price * fill.Quantity));
                        pos.ApplyFill(fill.Quantity, fill.Price);
                    }
                    ordersQueue.Clear();
                }
                prevBar = bar;
            }

            var prices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { _cfg.Symbol, prevBar?.Close ?? 0m }
            };

            var snap = new IO.PortfolioSnapshot
            {
                Cash = pf.Cash
            };
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

            var rpt = SummaryReporter.FromSnapshot(snap, prices);
            return rpt;
        }
    }
}