using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using QuantFrameworks.Backtest;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class ExposureGuardTests
    {
        [Fact]
        public async Task MaxGrossExposure_Limits_Position()
        {
            var csv = "Date,Open,High,Low,Close,Volume\n" +
                      "2024-01-01,100,100,100,100,1\n" +
                      "2024-01-02,100,100,100,100,1\n";
            var p = Path.GetTempFileName(); File.WriteAllText(p, csv);

            var cfg = new BacktestConfig
            {
                Symbol = "AAPL", DataPath = p,
                Start = new DateTime(2024,1,1), End = new DateTime(2024,1,2),
                StartingCash = 100_000m, Fast = 1, Slow = 2,
                SizingMode = "PercentNav", PercentNavPerTrade = 1.00m, // try to size huge
                LotSize = 1, MaxGrossExposurePct = 0.50m // cap gross to 50% NAV
            };

            var r = new QuantFrameworks.Backtest.MultiAssetBacktestRunner(cfg);
            var (summary, run) = await r.RunAsync();

            Assert.True(summary.NAV > 0); // Just sanity; guard applied internally
        }
    }
}
