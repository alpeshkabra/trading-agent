using Backtesting.Utils;

namespace Backtesting.Tools;

public static class SampleDataGenerator
{
    public static void WriteSampleCsv(string path, DateOnly start, int days, double s0 = 100, double mu = 0.07, double sigma = 0.2, int seed = 42)
    {
        var rnd = new Random(seed);
        var dt = 1.0 / 252.0;
        var rows = new List<string[]>();
        double s = s0;
        var date = start;

        rows.Add(new[] { "Date", "Open", "High", "Low", "Close", "Volume" });
        for (int i = 0; i < days; i++)
        {
            var z = Math.Sqrt(-2.0 * Math.Log(rnd.NextDouble())) * Math.Cos(2 * Math.PI * rnd.NextDouble());
            var ret = (mu - 0.5 * sigma * sigma) * dt + sigma * Math.Sqrt(dt) * z;
            var open = s;
            var close = s * Math.Exp(ret);
            var high = Math.Max(open, close) * (1 + 0.005 * rnd.NextDouble());
            var low  = Math.Min(open, close) * (1 - 0.005 * rnd.NextDouble());
            var vol  = 100000 + rnd.Next(0, 100000);
            rows.Add(new[] { date.ToString("yyyy-MM-dd"), open.ToString("F2"), high.ToString("F2"), low.ToString("F2"), close.ToString("F2"), vol.ToString() });
            s = close;
            date = date.AddDays(1);
        }

        using var sw = new StreamWriter(path);
        foreach (var r in rows) sw.WriteLine(string.Join(",", r));
    }
}
