using System.Globalization;
using Quant.Models;

namespace Quant
{
    /// <summary>
    /// Minimal CSV reader compatible with Program.cs expectations.
    /// CSV header (case-insensitive): Date,Open,High,Low,Close,Volume
    /// </summary>
    public sealed class CsvReader
    {
        private readonly string _path;
        public CsvReader(string path) => _path = path;

        /// <summary>
        /// Reads bars and applies optional [from, to] date filter (inclusive).
        /// </summary>
        public IEnumerable<Bar> ReadBars(DateOnly? from = null, DateOnly? to = null)
        {
            if (!File.Exists(_path))
                throw new FileNotFoundException("CSV not found", _path);

            using var sr = new StreamReader(_path);
            if (sr.EndOfStream) yield break;

            // Read header and build column map (case-insensitive)
            var header = (sr.ReadLine() ?? "");
            var cols = header.Split(',', StringSplitOptions.TrimEntries);
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cols.Length; i++) map[cols[i]] = i;

            int idx(string name)
            {
                if (!map.TryGetValue(name, out var ix))
                    throw new InvalidDataException($"CSV missing required column '{name}' in {_path}");
                return ix;
            }

            int iDate   = idx("Date");
            int iOpen   = map.ContainsKey("Open")   ? map["Open"]   : -1;
            int iHigh   = map.ContainsKey("High")   ? map["High"]   : -1;
            int iLow    = map.ContainsKey("Low")    ? map["Low"]    : -1;
            int iClose  = map.ContainsKey("Close")  ? map["Close"]  : -1;
            int iVolume = map.ContainsKey("Volume") ? map["Volume"] : -1;

            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < cols.Length) continue;

                // Parse date: accept yyyy-MM-dd or ISO-8601
                DateOnly d;
                var ds = parts[iDate];
                if (!DateOnly.TryParse(ds, CultureInfo.InvariantCulture, out d))
                {
                    // fall back to DateTime then convert
                    if (DateTime.TryParse(ds, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
                        d = DateOnly.FromDateTime(dt);
                    else
                        continue;
                }

                if (from.HasValue && d < from.Value) continue;
                if (to.HasValue   && d > to.Value)   continue;

                double rd(int ix, double def = double.NaN)
                {
                    if (ix < 0) return def;
                    var s = parts[ix];
                    return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : def;
                }

                var bar = new Bar
                {
                    Date   = d,
                    Open   = rd(iOpen),
                    High   = rd(iHigh),
                    Low    = rd(iLow),
                    Close  = rd(iClose),
                    Volume = rd(iVolume)
                };

                yield return bar;
            }
        }
    }
}
