using QuantFrameworks.Signals;
using Xunit;

namespace Quant.Tests.Signals
{
    public class SmaCalcTests
    {
        [Fact]
        public void SMA_Computes()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "Date,Open,High,Low,Close,Volume\n" +
                                   "2024-01-01,0,0,0,1,0\n" +
                                   "2024-01-02,0,0,0,2,0\n" +
                                   "2024-01-03,0,0,0,3,0\n");
            var cfg = new SignalConfig{ SmaFast=2 };
            var rows = IndicatorCalc.Compute(tmp, cfg);
            Assert.Null(rows[1].SmaFast);
            Assert.Equal(2.5, rows[2].SmaFast!.Value, 3);
        }
    }
}
