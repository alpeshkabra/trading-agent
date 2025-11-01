using System.IO;
using QuantFrameworks.Risk;
using Xunit;

namespace Quant.Tests.Risk
{
    public class RiskConfigTests
    {
        [Fact]
        public void Loads_Config()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "{\"baseCurrency\":\"USD\",\"blacklist\":[\"BAD\"],\"sizing\":{\"mode\":\"FixedFraction\",\"fixedFraction\":0.2,\"capital\":50000}}");
            var cfg = RiskConfig.Load(tmp);
            Assert.Equal("USD", cfg.BaseCurrency);
            Assert.Contains("BAD", cfg.Blacklist);
            Assert.Equal("FixedFraction", cfg.Sizing.Mode);
            Assert.Equal(0.2, cfg.Sizing.FixedFraction);
        }
    }
}
