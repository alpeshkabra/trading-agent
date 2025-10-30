using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace QuantFrameworks.Feeds
{
    /// Merge per-symbol CSVs (Date,Open,High,Low,Close,Volume) into a single time-ordered stream.
    public sealed class MultiCsvMarketDataFeed
    {
        private readonly Dictionary<string, string> _symbolToPath;
        public MultiCsvMarketDataFeed(Dictionary<string, string> symbolToPath) => _symbolToPath = symbolToPath;

        private static IEnumerable<Bar> ReadOne(string symbol, string path, DateTime start, DateTime end)
        {
            using var sr = new StreamReader(path);
            string? line = sr.ReadLine(); // header
            if (line is null) yield break;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var p = line.Split(',');
                if (p.Length < 6) continue;
                if (!DateTime.TryParse(p[0], CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dt)) continue;
                if (dt < start || dt > end) continue;

                var open  = decimal.Parse(p[1], CultureInfo.InvariantCulture);
                var high  = decimal.Parse(p[2], CultureInfo.InvariantCulture);
                var low   = decimal.Parse(p[3], CultureInfo.InvariantCulture);
                var close = decimal.Parse(p[4], CultureInfo.InvariantCulture);
                var vol   = long.Parse(p[5], CultureInfo.InvariantCulture);
                yield return new Bar(dt, symbol, open, high, low, close, vol);
            }
        }

        public IEnumerable<Bar> ReadMerged(DateTime start, DateTime end)
        {
            var enums = new List<IEnumerator<Bar>>();
            try
            {
                foreach (var kv in _symbolToPath)
                    enums.Add(ReadOne(kv.Key, kv.Value, start, end).GetEnumerator());

                var heap = new List<(Bar bar, int idx)>();
                for (int i = 0; i < enums.Count; i++)
                    if (enums[i].MoveNext()) heap.Add((enums[i].Current, i));

                while (heap.Count > 0)
                {
                    int minAt = 0;
                    for (int i = 1; i < heap.Count; i++)
                        if (heap[i].bar.Date < heap[minAt].bar.Date) minAt = i;

                    var (bar, idx) = heap[minAt];
                    yield return bar;

                    heap.RemoveAt(minAt);
                    if (enums[idx].MoveNext())
                        heap.Add((enums[idx].Current, idx));
                }
            }
            finally { foreach (var e in enums) e.Dispose(); }
        }
    }
}
