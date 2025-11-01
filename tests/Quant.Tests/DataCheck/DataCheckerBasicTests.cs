using QuantFrameworks.DataCheck;
using Xunit;

namespace Quant.Tests.DataCheck
{
    public class DataCheckerBasicTests
    {
        [Fact]
        public void Detects_Outlier_And_ZeroVolume()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp,
@"Date,Open,High,Low,Close,Volume
2024-01-02,100,101,99,100,10
2024-01-03,100,101,99,130,0
");
            var cfg = new DqxConfig { MaxAbsReturn = 0.2, MinVolume = 1, MaxGapDays = 3 };
            var (sum, anomalies) = DataChecker.CheckCsv(tmp, cfg);
            Assert.Equal(2, sum.Rows);
            Assert.True(sum.Outliers >= 1);
            Assert.True(sum.ZeroOrNegative >= 1);
            Assert.True(anomalies.Any(a => a.Kind == AnomalyKind.Outlier));
        }
    }
}
