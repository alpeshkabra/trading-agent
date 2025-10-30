using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuantFrameworks.Feeds;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class MultiFeedTests
    {
        [Fact]
        public void Merges_Two_Symbols_By_Date()
        {
            var a = "Date,Open,High,Low,Close,Volume\n2024-01-01,10,10,10,10,1\n2024-01-03,12,12,12,12,1\n";
            var b = "Date,Open,High,Low,Close,Volume\n2024-01-02,20,20,20,20,1\n2024-01-03,21,21,21,21,1\n";
            var pa = Path.GetTempFileName(); File.WriteAllText(pa, a);
            var pb = Path.GetTempFileName(); File.WriteAllText(pb, b);

            var feed = new MultiCsvMarketDataFeed(new Dictionary<string, string> { { "AAPL", pa }, { "MSFT", pb } });
            var merged = feed.ReadMerged(new DateTime(2024, 1, 1), new DateTime(2024, 1, 5)).ToList();

            Assert.Equal(4, merged.Count);
            Assert.Equal("AAPL", merged[0].Symbol);
            Assert.Equal("MSFT", merged[1].Symbol);
            var lastTwo = new[] { merged[2].Symbol, merged[3].Symbol };
            Assert.Contains("AAPL", lastTwo);
            Assert.Contains("MSFT", lastTwo);

        }
    }
}
