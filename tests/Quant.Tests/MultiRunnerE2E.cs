using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using QuantFrameworks.Backtest;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class MultiRunnerE2E
    {
        [Fact]
        public async Task Runs_Multi_Asset_And_Emits_Daily_NAV()
        {
            var a = "Date,Open,High,Low,Close,Volume\n" +
                    "2024-01-01,10,10,10,10,1\n" +
                    "2024-01-02,11,11,11,11,1\n" +
                    "2024-01-03,10,10,10,10,1\n";
            var b = "Date,Open,High,Low,Close,Volume\n" +
                    "2024-01-01,20,20,20,20,1\n" +
                    "2024-01-02,21,21,21,21,1\n" +
                    "2024-01-03,19,19,19,19,1\n";
            var pa = Path.GetTempFileName(); File.WriteAllText(pa, a);
            var pb = Path.GetTempFileName(); File.WriteAllText(pb, b);

            var cfg = new BacktestConfig
            {
                Symbols = new System.Collections.Generic.List<string>{ "AAPL", "MSFT" },
                SymbolData = new System.Collections.Generic.Dictionary<string,string>{{"AAPL",pa},{"MSFT",pb}},
                Start = new DateTime(2024,1,1),
                End = new DateTime(2024,1,3),
                StartingCash = 100_000m,
                Fast = 1, Slow = 2,
                SlippageBps = 25m,
                OutputPath = Path.Combine(Path.GetTempPath(), $"summary_{Guid.NewGuid():N}.csv"),
                DailyNavCsv = Path.Combine(Path.GetTempPath(), $"nav_{Guid.NewGuid():N}.csv"),
                RunJson = Path.Combine(Path.GetTempPath(), $"run_{Guid.NewGuid():N}.json")
            };

            var runner = new MultiAssetBacktestRunner(cfg);
            var (summary, run) = await runner.RunAsync();

            Assert.True(run.DailyNav.Count >= 2);
            Assert.True(summary.NAV > 0);
        }
    }
}
