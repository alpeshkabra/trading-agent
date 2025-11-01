using QuantFrameworks.Beta;
using Xunit;

namespace Quant.Tests.Beta
{
    public class BetaCliParseTests
    {
        [Fact]
        public void Parses_Spec()
        {
            var d = BetaCli.ParseSymbols("AAPL=/a.csv,MSFT=/b.csv");
            Assert.Equal("/b.csv", d["MSFT"]);
        }
    }
}
