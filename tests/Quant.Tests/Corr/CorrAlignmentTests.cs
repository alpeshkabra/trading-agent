using QuantFrameworks.Corr;
using Xunit;

namespace Quant.Tests.Corr
{
    public class CorrAlignmentTests
    {
        [Fact]
        public void Intersects_Dates_Across_Symbols()
        {
            var dir = Directory.CreateTempSubdirectory();
            string a = Path.Combine(dir.FullName, "a.csv");
            string b = Path.Combine(dir.FullName, "b.csv");
            File.WriteAllText(a,
@"Date,Open,High,Low,Close,Volume
2024-01-01,0,0,0,100,0
2024-01-02,0,0,0,101,0
2024-01-03,0,0,0,102,0
");
            File.WriteAllText(b,
@"Date,Open,High,Low,Close,Volume
2024-01-02,0,0,0,200,0
2024-01-03,0,0,0,201,0
2024-01-04,0,0,0,202,0
");
            var dict = new Dictionary<string,string>{{"A", a},{"B", b}};
            var (dates, rets) = CorrCalc.LoadAlignedReturns(dict);
            // intersection is 2024-01-02, 2024-01-03
            Assert.Single(dates);
            Assert.Equal(new DateOnly(2024,1,3), dates.Single());
        }
    }
}
