using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace QuantFrameworks.Feeds
{
    /// <summary>Reads a multi-symbol CSV file with header: Date,Symbol,Open,High,Low,Close,Volume</summary>
    public sealed class CsvMarketDataFeed : IMarketDataFeed
    {
        private readonly string _path;
        public CsvMarketDataFeed(string path) => _path = path;

        public async IAsyncEnumerable<Bar> ReadAsync(string symbol, DateTime start, DateTime end, [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var sr = new StreamReader(_path);
            string? line = await sr.ReadLineAsync(); // header
            if (line is null) yield break;

            while (!sr.EndOfStream && !ct.IsCancellationRequested)
            {
                line = await sr.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var p = line.Split(',');
                if (p.Length < 7) continue;

                if (!DateTime.TryParse(p[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)) continue;
                if (!string.Equals(p[1].Trim(), symbol, StringComparison.OrdinalIgnoreCase)) continue;
                if (dt < start || dt > end) continue;

                var open = decimal.Parse(p[2], CultureInfo.InvariantCulture);
                var high = decimal.Parse(p[3], CultureInfo.InvariantCulture);
                var low  = decimal.Parse(p[4], CultureInfo.InvariantCulture);
                var close= decimal.Parse(p[5], CultureInfo.InvariantCulture);
                var vol  = long.Parse(p[6], CultureInfo.InvariantCulture);

                yield return new Bar(dt, symbol, open, high, low, close, vol);
            }
        }
    }
}