using QuantFrameworks.Corr;
using Xunit;

namespace Quant.Tests.Corr
{
    public class CorrCalcOppositeTests
    {
        [Fact]
        public void Opposite_Trends_Negative_Corr()
        {
            var dir = Directory.CreateTempSubdirectory();
            string up = Path.Combine(dir.FullName, "up.csv");
            string dn = Path.Combine(dir.FullName, "dn.csv");

            // Geometric growth/decay so returns are constant +10% vs -10%
            File.WriteAllText(up,
                            @"Date,Open,High,Low,Close,Volume
                            2024-01-01,0,0,0,100,0
                            2024-01-02,0,0,0,110,0
                            2024-01-03,0,0,0,121,0
                            2024-01-04,0,0,0,133.1,0
                            2024-01-05,0,0,0,146.41,0
                            ");
                                        File.WriteAllText(dn,
                            @"Date,Open,High,Low,Close,Volume
                            2024-01-01,0,0,0,100,0
                            2024-01-02,0,0,0,90,0
                            2024-01-03,0,0,0,81,0
                            2024-01-04,0,0,0,72.9,0
                            2024-01-05,0,0,0,65.61,0
                            ");

            var dict = new Dictionary<string,string>{{"UP", up},{"DN", dn}};
            var (dates, rets) = CorrCalc.LoadAlignedReturns(dict);
            var rows = CorrCalc.RollingPairwiseCorr(dates, rets, window:3).ToList();

            // Expect strongly negative correlation (≈ -1)
            Assert.Contains(rows, r => r.s1 == "DN" && r.s2 == "UP" && r.corr < -0.95);
        }
    }
}
