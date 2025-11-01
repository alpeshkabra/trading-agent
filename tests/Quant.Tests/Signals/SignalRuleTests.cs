using QuantFrameworks.Signals;
using Xunit;

namespace Quant.Tests.Signals
{
    public class SignalRuleTests
    {
        [Fact]
        public void SMA_Cross_Generates_Buy_Sell()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "Date,Open,High,Low,Close,Volume\n" +
                                   "2024-01-01,0,0,0,10,0\n" +
                                   "2024-01-02,0,0,0,11,0\n" +
                                   "2024-01-03,0,0,0,12,0\n" +
                                   "2024-01-04,0,0,0,11,0\n" +
                                   "2024-01-05,0,0,0,9,0\n" +
                                   "2024-01-06,0,0,0,8,0\n");
            var cfg = new SignalConfig{ SmaFast=2, SmaSlow=3 };
            var rows = IndicatorCalc.Compute(tmp, cfg);
            var sigs = SignalRules.Generate(rows, cfg);
            Assert.Contains(sigs, s => s.Signal == TradeSignal.BUY  || s.Signal == TradeSignal.SELL);
        }
    }
}
