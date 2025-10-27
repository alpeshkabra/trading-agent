using Quant.Data;
using Quant.Models;
using Quant.Analytics;
using Quant.Reports;

namespace Quant;

public static class Program
{
    private static void Help()
    {
        Console.WriteLine(@"
QuantFrameworks - CSV reader + SPY vs stocks daily return comparison

Usage:
  dotnet run -- --spy <path-to-SPY.csv> --stocks <CSV1,CSV2,...> [--from YYYY-MM-DD] [--to YYYY-MM-DD]
              [--field Close] [--out reports]

CSV format (OHLCV daily, header required):
  Date,Open,High,Low,Close,Volume

Example:
  dotnet run -- --spy data/SPY.csv --stocks data/AAPL.csv,data/MSFT.csv --from 2018-01-01 --out reports
");
    }

    public static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            Help();
            return 0;
        }

        string? spyPath = GetArg(args, "spy");
        string? stockList = GetArg(args, "stocks");
        string field = GetArg(args, "field", "Close")!;
        string outDir = GetArg(args, "out", "reports")!;
        DateOnly? from = ParseDate(GetArg(args, "from"));
        DateOnly? to = ParseDate(GetArg(args, "to"));

        if (string.IsNullOrWhiteSpace(spyPath) || string.IsNullOrWhiteSpace(stockList))
        {
            Console.Error.WriteLine("ERROR: --spy and --stocks are required.");
            Help();
            return 2;
        }

        Directory.CreateDirectory(outDir);

        var spy = new CsvReader(spyPath).ReadBars(from, to).ToList();
        if (spy.Count == 0) { Console.Error.WriteLine("No rows loaded for SPY."); return 3; }
        var spySeries = spy.Select(b => new PricePoint(b.Date, PickField(b, field))).ToList();
        var spyReturns = Returns.Simple(spySeries).ToList();

        var stockPaths = stockList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var path in stockPaths)
        {
            var bars = new CsvReader(path).ReadBars(from, to).ToList();
            if (bars.Count == 0) { Console.WriteLine($"WARN: no rows for {path}"); continue; }

            var ticker = InferTickerFromPath(path);
            var px = bars.Select(b => new PricePoint(b.Date, PickField(b, field))).ToList();
            var rets = Returns.Simple(px).ToList();

            var (spyAligned, stockAligned) = Aligner.AlignByDate(spyReturns, rets);

            var rows = new List<string[]>();
            rows.Add(new[] { "Date", "SPY_Return", $"{ticker}_Return", $"{ticker}_Excess" });
            for (int i = 0; i < spyAligned.Count; i++)
            {
                var d = spyAligned[i].Date;
                var rSpy = spyAligned[i].Return;
                var rStk = stockAligned[i].Return;
                var excess = rStk - rSpy;
                rows.Add(new[] { d.ToString("yyyy-MM-dd"), rSpy.ToString("F6"), rStk.ToString("F6"), excess.ToString("F6") });
            }

            var outPath = Path.Combine(outDir, $"compare_{ticker}_vs_SPY.csv");
            CsvWriter.Write(outPath, rows);
            Console.WriteLine($"Wrote {outPath} ({rows.Count-1} rows).");

            var avgSpy = spyAligned.Average(p => p.Return);
            var avgStk = stockAligned.Average(p => p.Return);
            var corr = Stats.Correlation(spyAligned.Select(p => p.Return), stockAligned.Select(p => p.Return));
            Console.WriteLine($"Summary {ticker}: meanRet={avgStk:F6}, meanSPY={avgSpy:F6}, corr={corr:F4}");
        }

        return 0;
    }

    private static string? GetArg(string[] args, string name, string? def = null)
    {
        var i = Array.FindIndex(args, a => a == $"--{name}");
        if (i >= 0 && i + 1 < args.Length) return args[i + 1];
        return def;
    }

    private static DateOnly? ParseDate(string? s) => DateOnly.TryParse(s, out var d) ? d : null;

    private static string InferTickerFromPath(string path)
    {
        var file = Path.GetFileNameWithoutExtension(path);
        if (file.Contains('_')) file = file.Split('_').Last();
        return file.ToUpperInvariant();
    }

    private static double PickField(Bar b, string field)
    {
        return field.ToLowerInvariant() switch
        {
            "open" => b.Open,
            "high" => b.High,
            "low" => b.Low,
            "close" => b.Close,
            _ => b.Close
        };
    }
}
