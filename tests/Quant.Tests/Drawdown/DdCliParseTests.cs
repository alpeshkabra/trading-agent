using QuantFrameworks.Drawdown;
using Xunit;

namespace Quant.Tests.Drawdown
{
    public class DdCliParseTests
    {
        [Fact]
        public void Parses_Spec()
        {
            var d = DdCli.ParseSymbols("AAPL=/a.csv,MSFT=/b.csv");
            Assert.Equal("/b.csv", d["MSFT"]);
        }
    }
}
