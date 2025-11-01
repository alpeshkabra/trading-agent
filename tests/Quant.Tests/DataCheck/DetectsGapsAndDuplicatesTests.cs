using QuantFrameworks.DataCheck;
using Xunit;

namespace Quant.Tests.DataCheck
{
    public class DetectsGapsAndDuplicatesTests
    {
        [Fact]
        public void Detects_Gap_And_Duplicate()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp,
@"Date,Open,High,Low,Close,Volume
2024-01-02,100,101,99,100,10
2024-01-02,100,101,99,100,10
2024-01-10,100,101,99,101,10
");
            var cfg = new DqxConfig { MaxGapDays = 3, MaxAbsReturn = 0.5, MinVolume = 1 };
            var (sum, _) = DataChecker.CheckCsv(tmp, cfg);
            Assert.Equal(1, sum.Duplicates);
            Assert.Equal(1, sum.Gaps);
        }
    }
}
