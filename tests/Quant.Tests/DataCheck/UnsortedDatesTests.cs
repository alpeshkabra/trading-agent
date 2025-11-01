using QuantFrameworks.DataCheck;
using Xunit;

namespace Quant.Tests.DataCheck
{
    public class UnsortedDatesTests
    {
        [Fact]
        public void Detects_Unsorted()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp,
@"Date,Open,High,Low,Close,Volume
2024-01-05,100,101,99,100,10
2024-01-03,100,101,99,100,10
2024-01-04,100,101,99,100,10
");
            var cfg = new DqxConfig();
            var (sum, _) = DataChecker.CheckCsv(tmp, cfg);
            Assert.True(sum.Unsorted >= 1);
        }
    }
}
