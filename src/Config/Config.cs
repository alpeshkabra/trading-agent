using Backtesting.Utils;

namespace Backtesting;

public class Config
{
    public string DataPath { get; init; } = "";
    public string? Symbol { get; init; } = "SYMB";
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public double Cash { get; init; } = 100_000;
    public string Strategy { get; init; } = "mac";
    public int Fast { get; init; } = 10;
    public int Slow { get; init; } = 30;
    public double Slippage { get; init; } = 0.0;
    public string? ReportDir { get; init; } = "reports";

    public static Config FromArgs(string[] args)
    {
        string? Get(string name, string? def = null)
        {
            var idx = Array.FindIndex(args, a => a == $"--{name}");
            if (idx >= 0 && idx + 1 < args.Length) return args[idx + 1];
            return def;
        }

        DateOnly? ParseDate(string? s) => DateOnly.TryParse(s, out var d) ? d : null;

        return new Config
        {
            DataPath = Get("data") ?? throw new ArgumentException("--data is required"),
            Symbol = Get("symbol", "SYMB"),
            From = ParseDate(Get("from")),
            To = ParseDate(Get("to")),
            Cash = double.TryParse(Get("cash", "100000"), out var cash) ? cash : 100000,
            Strategy = Get("strategy", "mac")!,
            Fast = int.TryParse(Get("fast", "10"), out var f) ? f : 10,
            Slow = int.TryParse(Get("slow", "30"), out var s) ? s : 30,
            Slippage = double.TryParse(Get("slippage", "0"), out var sl) ? sl : 0,
            ReportDir = Get("report", "reports")
        };
    }
}
