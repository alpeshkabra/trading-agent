using QuantFrameworks.Signals;
using Xunit;

namespace Quant.Tests.Signals
{
    public class BollingerTests
    {
        [Fact]
        public void Bands_Computed()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "Date,Open,High,Low,Close,Volume\n" +
                                   "2024-01-01,0,0,0,100,0\n" +
                                   "2024-01-02,0,0,0,101,0\n" +
                                   "2024-01-03,0,0,0,102,0\n" +
                                   "2024-01-04,0,0,0,103,0\n" +
                                   "2024-01-05,0,0,0,104,0\n");
            var rows = IndicatorCalc.Compute(tmp, new SignalConfig{ Bb=5, BbStd=2 });
            Assert.NotNull(rows.Last().BbUpper);
            Assert.NotNull(rows.Last().BbLower);
        }
    }
}
