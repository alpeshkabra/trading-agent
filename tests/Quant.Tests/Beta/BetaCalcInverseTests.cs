using QuantFrameworks.Beta;
using Xunit;

namespace Quant.Tests.Beta
{
    public class BetaCalcInverseTests
    {
        [Fact]
        public void Inverse_Series_Beta_Negative()
        {
            var dir = Directory.CreateTempSubdirectory();
            string b = Path.Combine(dir.FullName, "b.csv");
            string a = Path.Combine(dir.FullName, "a.csv");

            // benchmark geometric +10%, asset geometric -10%
            File.WriteAllText(b,
                @"Date,Open,High,Low,Close,Volume
                2024-01-01,0,0,0,100,0
                2024-01-02,0,0,0,110,0
                2024-01-03,0,0,0,121,0
                2024-01-04,0,0,0,133.1,0
                2024-01-05,0,0,0,146.41,0
                ");
                            File.WriteAllText(a,
                @"Date,Open,High,Low,Close,Volume
                2024-01-01,0,0,0,100,0
                2024-01-02,0,0,0,90,0
                2024-01-03,0,0,0,81,0
                2024-01-04,0,0,0,72.9,0
                2024-01-05,0,0,0,65.61,0
                ");
            var syms = new Dictionary<string,string>{{"INV", a}};
            var (dates, bench, asset) = BetaCalc.LoadAlignedReturns(b, syms);
            var rows = BetaCalc.Rolling(dates, bench, asset, window:3).ToList();
            Assert.Contains(rows, r => r.sym=="INV" && r.beta < -0.95);
        }
    }
}
