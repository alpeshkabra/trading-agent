using QuantFrameworks.Models;
using QuantFrameworks.Risk;
using Xunit;

namespace Quant.Tests.Risk
{
    public class RiskEngineRulesTests
    {
        [Fact]
        public void Rejects_Blacklisted()
        {
            var cfg = new RiskConfig{ Blacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "BAD" } };
            var eng = new RiskEngine(cfg, new());
            var r = eng.Evaluate(new Order(DateTime.UtcNow, "BAD", "BUY", 10, 50m));
            Assert.False(r.Approved);
        }

        [Fact]
        public void Clamps_PerSymbolQty()
        {
            var cfg = new RiskConfig{
                Sizing = new SizingConfig{ Mode="FixedFraction", FixedFraction=1.0, Capital=1000 },
                PerSymbolMaxQty = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase){{"ABC",5}},
            };
            var eng = new RiskEngine(cfg, new());
            var r = eng.Evaluate(new Order(DateTime.UtcNow, "ABC", "BUY", 0, 10m));
            Assert.True(r.Approved);
            Assert.Equal(5, r.FinalQty); // clamped to per-symbol limit
        }
    }
}
