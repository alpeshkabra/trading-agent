using System;
using QuantFrameworks.Sizing;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class PositionSizingTests
    {
        [Fact]
        public void FixedDollarSizer_Respects_LotSize()
        {
            var s = new FixedDollarSizer(10_000m, lotSize: 100);
            var qty = s.Size(105m, nav: 100_000m);
            Assert.True(qty % 100 == 0);
            Assert.True(qty > 0);
        }

        [Fact]
        public void PercentNavSizer_Computes_From_NAV()
        {
            var s = new PercentNavSizer(0.05m, lotSize: 10);
            var qty = s.Size(50m, nav: 200_000m); // 5% of 200k = 10k => 200 shares => round to 10
            Assert.Equal(200, qty);
        }
    }
}
