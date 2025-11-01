using System.Globalization;
using QuantFrameworks.Models;

namespace QuantFrameworks.IO
{
    public static class CsvReaders
    {
        public static List<Order> ReadOrders(string path)
        {
            var list = new List<Order>();
            using var sr = new StreamReader(path);
            _ = sr.ReadLine(); // header
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 5) continue;

                var ts = DateTime.Parse(parts[0], null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                var symbol = parts[1];
                var side = parts[2];
                var qty = string.IsNullOrWhiteSpace(parts[3]) ? 0 : int.Parse(parts[3], CultureInfo.InvariantCulture);
                var price = decimal.Parse(parts[4], CultureInfo.InvariantCulture);

                list.Add(new Order(ts, symbol, side, qty, price));
            }
            return list;
        }

        // (date, symbol) -> close
        public static Dictionary<(DateOnly date, string symbol), decimal> ReadDailyCloses(string path)
        {
            var dict = new Dictionary<(DateOnly, string), decimal>();
            using var sr = new StreamReader(path);
            _ = sr.ReadLine(); // header
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 3) continue;

                var date = DateOnly.ParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var symbol = parts[1];
                var close = decimal.Parse(parts[2], CultureInfo.InvariantCulture);

                dict[(date, symbol)] = close;
            }
            return dict;
        }
    }
}
