using QuantFrameworks.Beta;
using Xunit;

namespace Quant.Tests.Beta
{
    public class BetaCalcIdentityTests
    {
        [Fact]
        public void Asset_Equals_Benchmark_Beta_One()
        {
            var dir = Directory.CreateTempSubdirectory();
            string p = Path.Combine(dir.FullName, "p.csv");
            File.WriteAllText(p,
                            @"Date,Open,High,Low,Close,Volume
                            2024-01-01,0,0,0,100,0
                            2024-01-02,0,0,0,105,0
                            2024-01-03,0,0,0,110,0
                            2024-01-04,0,0,0,120,0
                            2024-01-05,0,0,0,115,0
                            ");
            var syms = new Dictionary<string,string>{{"S", p}};
            var (dates, bench, asset) = BetaCalc.LoadAlignedReturns(p, syms);
            var rows = BetaCalc.Rolling(dates, bench, asset, window:3).ToList();
            Assert.Contains(rows, r => r.sym=="S" && r.beta > 0.95 && r.beta < 1.05);
        }
    }
}
