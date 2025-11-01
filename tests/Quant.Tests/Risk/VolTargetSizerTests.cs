using QuantFrameworks.Models;
using QuantFrameworks.Risk;
using QuantFrameworks.Risk.Sizing;
using Xunit;

namespace Quant.Tests.Risk
{
    public class VolTargetSizerTests
    {
        [Fact]
        public void Sizes_WithVolTarget()
        {
            var cfg = new RiskConfig{ Sizing = new SizingConfig{ Mode="VolTarget", VolTargetAnnual=0.2, Capital=100000, LookbackDays=5 } };
            var px = new Dictionary<(DateOnly,string), decimal>();
            var dates = new[]{ new DateOnly(2024,1,1), new DateOnly(2024,1,2), new DateOnly(2024,1,3), new DateOnly(2024,1,4), new DateOnly(2024,1,5), new DateOnly(2024,1,6) };
            decimal[] closes = {100,102,101,103,104,105};
            for(int i=0;i<dates.Length;i++) px[(dates[i],"ABC")] = closes[i];

            var sizer = new VolTargetSizer(cfg, px);
            var q = sizer.Size(new Order(DateTime.UtcNow, "ABC", "BUY", 0, 100m));
            Assert.True(q >= 0);
        }
    }
}
