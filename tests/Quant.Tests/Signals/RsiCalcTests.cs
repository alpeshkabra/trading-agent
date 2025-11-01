using QuantFrameworks.Signals;
using Xunit;

namespace Quant.Tests.Signals
{
    public class RsiCalcTests
    {
        [Fact]
        public void RSI_Computes()
        {
            var tmp = Path.GetTempFileName();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Date,Open,High,Low,Close,Volume");
            double px = 100;
            for (int i=0;i<20;i++) { px += (i%2==0?1:-0.5); sb.AppendLine($"2024-01-{i+1:00},0,0,0,{px},0"); }
            File.WriteAllText(tmp, sb.ToString());

            var rows = IndicatorCalc.Compute(tmp, new SignalConfig{ Rsi=14 });
            Assert.Contains(rows, r => r.Rsi.HasValue);
        }
    }
}
