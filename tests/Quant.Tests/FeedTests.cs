using System;
using System.IO;
using System.Threading.Tasks;
using QuantFrameworks.Feeds;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class FeedTests
    {
        [Fact]
        public async Task CsvFeed_Filters_By_Symbol_And_Range()
        {
            var csv = "Date,Symbol,Open,High,Low,Close,Volume\n" +
                      "2024-01-02,AAPL,1,2,0.5,1.5,1000\n" +
                      "2024-01-02,MSFT,1,2,0.5,1.5,1000\n";
            var path = Path.GetTempFileName();
            await File.WriteAllTextAsync(path, csv);

            var feed = new CsvMarketDataFeed(path);
            var bars = feed.ReadAsync("AAPL", new DateTime(2024,1,1), new DateTime(2024,1,3));
            int count = 0;
            await foreach (var _ in bars) count++;

            Assert.Equal(1, count);
        }
    }
}