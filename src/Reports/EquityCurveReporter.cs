using Backtesting.Utils;

namespace Backtesting.Reports;

public class EquityCurveReporter
{
    public void WriteCsv(string path, IEnumerable<Engine.EquityPoint> points)
    {
        Csv.Write(path, points.Select(p => new[] { p.Date.ToString("yyyy-MM-dd"), p.Equity.ToString("F2") }),
            header: new[] { "Date", "Equity" });
    }
}
