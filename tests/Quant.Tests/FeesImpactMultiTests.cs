using System;
using System.IO;
using System.Threading.Tasks;
using QuantFrameworks.Backtest;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class FeesImpactMultiTests
    {
        [Fact]
        public async Task Fees_Lower_NAV()
        {
            var a = "Date,Open,High,Low,Close,Volume\n" +
                    "2024-01-01,10,10,10,10,1\n" +
                    "2024-01-02,11,11,11,11,1\n";
            var pa = Path.GetTempFileName(); File.WriteAllText(pa, a);

            var cfgNoFees = new BacktestConfig
            {
                Symbol = "AAPL",
                DataPath = pa,
                Start = new DateTime(2024,1,1),
                End = new DateTime(2024,1,2),
                StartingCash = 100_000m,
                Fast = 1, Slow = 2
            };

            var cfgWithFees = new BacktestConfig
            {
                Symbol = "AAPL",
                DataPath = pa,
                Start = new DateTime(2024,1,1),
                End = new DateTime(2024,1,2),
                StartingCash = 100_000m,
                Fast = 1, Slow = 2,
                CommissionPerOrder = 1.00m,
                PercentFee = 0.002m
            };

            var r1 = new MultiAssetBacktestRunner(cfgNoFees);
            var (s1, _) = await r1.RunAsync();

            var r2 = new MultiAssetBacktestRunner(cfgWithFees);
            var (s2, _) = await r2.RunAsync();

            Assert.True(s2.NAV <= s1.NAV);
        }
    }
}
