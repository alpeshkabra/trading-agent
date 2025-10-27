using Backtesting.Core;
using Backtesting.Utils;

namespace Backtesting.Data;

public class CsvDataProvider : IDataProvider
{
    private readonly string _path;
    public CsvDataProvider(string path) => _path = path;

    public IEnumerable<Bar> LoadBars(DateOnly? from = null, DateOnly? to = null)
    {
        foreach (var row in Csv.Read(_path, hasHeader: true))
        {
            if (!DateOnly.TryParse(row[0], out var date)) continue;
            if (from is not null && date < from) continue;
            if (to is not null && date > to) continue;
            double open = double.Parse(row[1]);
            double high = double.Parse(row[2]);
            double low  = double.Parse(row[3]);
            double close= double.Parse(row[4]);
            double vol  = row.Length > 5 ? double.Parse(row[5]) : 0;
            yield return new Bar(date, open, high, low, close, vol);
        }
    }
}
