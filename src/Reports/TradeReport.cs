using Backtesting.Models;
using Backtesting.Utils;

namespace Backtesting.Reports;

public static class TradeReport
{
    public static void WriteCsv(string path, IReadOnlyList<Trade> trades)
    {
        Csv.Write(path, trades.Select(t => new[]
        {
            t.Date.ToString("yyyy-MM-dd"),
            t.Side.ToString(),
            t.Quantity.ToString("F4"),
            t.Price.ToString("F4"),
            t.Tag
        }), header: new[] { "Date","Side","Qty","Price","Tag" });
    }
}
