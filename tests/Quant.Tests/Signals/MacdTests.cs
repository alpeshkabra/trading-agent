using QuantFrameworks.Signals;
using Xunit;

namespace Quant.Tests.Signals
{
    public class MacdTests
    {
        [Fact]
        public void MACD_Computes()
        {
            var tmp = Path.GetTempFileName();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Date,Open,High,Low,Close,Volume");
            for (int i=0;i<40;i++) sb.AppendLine($"2024-01-{i+1:00},0,0,0,{100+i},0");
            File.WriteAllText(tmp, sb.ToString());

            var rows = IndicatorCalc.Compute(tmp, new SignalConfig{ MacdFast=12, MacdSlow=26, MacdSignal=9 });
            Assert.Contains(rows, r => r.Macd.HasValue && r.MacdSignal.HasValue);

        }
    }
}
