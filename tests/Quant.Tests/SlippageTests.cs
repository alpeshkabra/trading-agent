using QuantFrameworks.Execution;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class SlippageTests
    {
        [Fact]
        public void Applies_Bps_Correctly()
        {
            var buyPx = SimpleSlippage.Apply(100m, +100, 50m);
            var sellPx = SimpleSlippage.Apply(100m, -100, 50m);
            Assert.True(buyPx > 100m);
            Assert.True(sellPx < 100m);
        }
    }
}
