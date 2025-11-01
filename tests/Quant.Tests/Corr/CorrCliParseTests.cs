using QuantFrameworks.Corr;
using Xunit;

namespace Quant.Tests.Corr
{
    public class CorrCliParseTests
    {
        [Fact]
        public void Parses_Spec()
        {
            var dict = CorrCli.ParseSymbols("AAPL=/a.csv,MSFT=/b.csv,SPY=/c.csv");
            Assert.Equal(3, dict.Count);
            Assert.Equal("/b.csv", dict["MSFT"]);
        }
    }
}
