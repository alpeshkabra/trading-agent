using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using QuantFrameworks.Backtest;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class BacktestRunnerE2ETests
    {
        [Fact]
        public async Task Runner_Completes_And_Produces_Summary()
        {
            var prices = "Date,Symbol,Open,High,Low,Close,Volume\n" +
                         "2024-01-01,AAPL,10,10,10,10,1000\n" +
                         "2024-01-02,AAPL,11,11,11,11,1000\n" +
                         "2024-01-03,AAPL,12,12,12,12,1000\n" +
                         "2024-01-04,AAPL,11,11,11,11,1000\n" +
                         "2024-01-05,AAPL,9,9,9,9,1000\n";
            var dataPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(dataPath, prices);

            var cfg = new BacktestConfig
            {
                DataPath = dataPath,
                Symbol = "AAPL",
                Start = new DateTime(2024,1,1),
                End = new DateTime(2024,1,5),
                StartingCash = 100_000m,
                Fast = 2, Slow = 3,
                OutputPath = Path.Combine(Path.GetTempPath(), $"summary_{Guid.NewGuid():N}.csv")
            };

            var runner = new BacktestRunner(cfg);
            var rpt = await runner.RunAsync();

            Assert.True(rpt.NAV > 0);
        }
    }
}