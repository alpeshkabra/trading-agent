using QuantFrameworks.Corr;
using Xunit;

namespace Quant.Tests.Corr
{
    public class CorrCalcIdentityTests
    {
        [Fact]
        public void Identity_Series_Have_Corr_One()
        {
            var dir = Directory.CreateTempSubdirectory();
            string a = Path.Combine(dir.FullName, "a.csv");
            File.WriteAllText(a,
@"Date,Open,High,Low,Close,Volume
2024-01-01,0,0,0,100,0
2024-01-02,0,0,0,101,0
2024-01-03,0,0,0,102,0
2024-01-04,0,0,0,103,0
2024-01-05,0,0,0,104,0
");
            var dict = new Dictionary<string,string>{{"AAPL", a},{"AAPL2", a}};
            var (dates, rets) = CorrCalc.LoadAlignedReturns(dict);
            var rows = CorrCalc.RollingPairwiseCorr(dates, rets, window:3).ToList();
            Assert.Contains(rows, r => r.corr > 0.999);
        }
    }
}
