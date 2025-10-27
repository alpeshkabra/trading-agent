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
            // header
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

                double open  = Parse(cols, 1);
                double high  = Parse(cols, 2);
                double low   = Parse(cols, 3);
                double close = Parse(cols, 4);
                double vol   = cols.Length > 5 ? Parse(cols, 5) : 0d;

                yield return new Bar(date, open, high, low, close, vol);
            }
        }

        private static double Parse(string[] cols, int idx)
        {
            // strip quotes safely; no raw strings used
            var s = cols[idx].Trim().Replace("\"", string.Empty);

            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;

            // fallback: current culture
            return double.Parse(s, NumberStyles.Any, CultureInfo.CurrentCulture);
        }
    }
}
