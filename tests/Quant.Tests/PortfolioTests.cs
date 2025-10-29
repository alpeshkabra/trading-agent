using QuantFrameworks.Portfolio;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class PortfolioTests
    {
        [Fact]
        public void AvgPrice_Updates_On_SameSide_Scale()
        {
            var p = new Position("AAPL");
            p.ApplyFill(100, 10m);
            p.ApplyFill(100, 12m);
            Assert.Equal(200, p.Quantity);
            Assert.Equal(11m, p.AvgPrice, 2);
        }

        [Fact]
        public void Reverse_Resets_AvgPrice()
        {
            var p = new Position("AAPL");
            p.ApplyFill(100, 10m);
            p.ApplyFill(-200, 9m); // cross short
            Assert.Equal(-100, p.Quantity);
            Assert.Equal(9m, p.AvgPrice);
        }
    }
}