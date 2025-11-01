using QuantFrameworks.Models;
using QuantFrameworks.Risk;
using QuantFrameworks.Risk.Sizing;
using Xunit;

namespace Quant.Tests.Risk
{
    public class FixedFractionSizerTests
    {
        [Fact]
        public void Sizes_ByBudget()
        {
            var cfg = new RiskConfig{ Sizing = new SizingConfig{ Mode="FixedFraction", FixedFraction=0.1, Capital=100000 } };
            var sizer = new FixedFractionSizer(cfg);
            var q = sizer.Size(new Order(DateTime.UtcNow, "ABC", "BUY", 0, 100m));
            Assert.Equal(100, q); // 10% of 100k at $100
        }
    }
}
