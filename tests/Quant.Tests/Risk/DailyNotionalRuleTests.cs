using QuantFrameworks.Models;
using QuantFrameworks.Risk;
using Xunit;

namespace Quant.Tests.Risk
{
    public class DailyNotionalRuleTests
    {
        [Fact]
        public void Blocks_Excess_DailyNotional()
        {
            var cfg = new RiskConfig{ MaxDailyNotional = 1000m, Sizing = new SizingConfig{ Mode="None" } };
            var eng = new RiskEngine(cfg, new());
            var ok = eng.Evaluate(new Order(DateTime.UtcNow, "ABC", "BUY", 5, 100m));
            var blocked = eng.Evaluate(new Order(DateTime.UtcNow, "XYZ", "BUY", 6, 100m));
            Assert.True(ok.Approved);
            Assert.False(blocked.Approved);
        }
    }
}
