using System;
using System.Collections.Generic;
using System.Threading;

namespace QuantFrameworks.Feeds
{
    public sealed record Bar(DateTime Date, string Symbol, decimal Open, decimal High, decimal Low, decimal Close, long Volume);

    public interface IMarketDataFeed
    {
        /// <summary>Stream bars in ascending Date for the requested symbol.</summary>
        IAsyncEnumerable<Bar> ReadAsync(string symbol, DateTime start, DateTime end, CancellationToken ct = default);
    }
}