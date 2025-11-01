using QuantFrameworks.DataCheck;
using Xunit;

namespace Quant.Tests.DataCheck
{
    public class ZeroNegativeTests
    {
        [Fact]
        public void Detects_ZeroOrNegative_Prices()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp,
@"Date,Open,High,Low,Close,Volume
2024-01-02,100,101,99,100,10
2024-01-03,-1,101,99,100,10
");
            var (sum, an) = DataChecker.CheckCsv(tmp, new DqxConfig());
            Assert.True(sum.ZeroOrNegative > 0);
            Assert.Contains(an, a => a.Kind == AnomalyKind.ZeroOrNegative);
        }
    }
}
