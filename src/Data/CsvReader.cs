using System.Globalization;
using Quant.Models;

namespace Quant.Data
{
    public class CsvReader
    {
        private readonly string _path;
        public CsvReader(string path) => _path = path;

        public IEnumerable<Bar> ReadBars(DateOnly? from = null, DateOnly? to = null)
        {
            using var sr = new StreamReader(_path);

            // Read and discard header
            _ = sr.ReadLine();

            string? line;
            while ((line = sr.ReadLine()) is not null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = line.Split(',');
                if (cols.Length < 5) continue;

                if (!DateOnly.TryParse(cols[0], out var date)) continue;
                if (from is not null && date < from) continue;
                if (to is not null && date > to) continue;

                double open  = ParseNumber(cols, 1);
                double high  = ParseNumber(cols, 2);
                double low   = ParseNumber(cols, 3);
                double close = ParseNumber(cols, 4);
                double vol   = cols.Length > 5 ? ParseNumber(cols, 5) : 0d;

                yield return new Bar(date, open, high, low, close, vol);
            }
        }

        private static double ParseNumber(string[] cols, int idx)
        {
            // Trim and unquote *safely* (no raw strings)
            var s = cols[idx].Trim();

            // If field is quoted, remove surrounding quotes and unescape double quotes
            if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
            {
                s = s.Substring(1, s.Length - 2).Replace("\"\"", "\"");
            }

            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;

            // Fallback to current culture if needed
            return double.Parse(s, NumberStyles.Any, CultureInfo.CurrentCulture);
        }
    }
}
