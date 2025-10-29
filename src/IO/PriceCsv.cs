using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace QuantFrameworks.IO
{
    public static class PriceCsv
    {
        public static Dictionary<string, decimal> ReadLatest(TextReader reader, DateTime asOf)
        {
            string? line = reader.ReadLine();
            if (line == null) throw new InvalidDataException("Empty prices CSV.");

            var latest = new Dictionary<string, (DateTime dt, decimal px)>(StringComparer.OrdinalIgnoreCase);

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 3) throw new InvalidDataException($"Bad price line: {line}");

                var dateStr = parts[0].Trim();
                var symbol = parts[1].Trim();
                var closeStr = parts[2].Trim();

                if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
                    continue;
                if (!decimal.TryParse(closeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var px))
                    continue;

                if (dt > asOf) continue;

                if (!latest.TryGetValue(symbol, out var cur) || dt > cur.dt)
                    latest[symbol] = (dt, px);
            }

            var map = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in latest)
                map[kv.Key] = kv.Value.px;

            return map;
        }
    }
}