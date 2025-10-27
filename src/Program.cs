using Backtesting.Core;
using Backtesting.Data;
using Backtesting.Engine;
using Backtesting.Models;
using Backtesting.Reports;
using Backtesting.Strategies;
using Backtesting.Utils;

namespace Backtesting;

public static class Program
{
    private static void PrintHelp()
    {
        Console.WriteLine(@"
BacktestingEngine - minimal yet extensible C# backtester

Usage:
  dotnet run -- --data <path-to-csv> --symbol <TICKER> [--from YYYY-MM-DD] [--to YYYY-MM-DD]
               [--cash 100000] [--strategy mac] [--fast 10] [--slow 30] [--slippage 0.0]
               [--report outdir]

CSV format (OHLCV daily):
  Date,Open,High,Low,Close,Volume

Example:
  dotnet run -- --data data/SPY.csv --symbol SPY --from 2015-01-01 --cash 100000 --strategy mac --fast 20 --slow 50 --report reports
");
    }

    public static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            PrintHelp();
            return 0;
        }

        var cfg = Config.FromArgs(args);
        Logger.Level = LogLevel.Info;

        try
        {
            if (cfg.ReportDir is not null)
            {
                Directory.CreateDirectory(cfg.ReportDir);
            }

            IDataProvider dataProvider = new CsvDataProvider(cfg.DataPath);
            var bars = dataProvider.LoadBars(cfg.From, cfg.To).ToList();
            if (bars.Count == 0)
            {
                Logger.Error("No data rows loaded. Check --data path and date filters.");
                return 2;
            }
            Logger.Info($"Loaded {bars.Count} bars from {cfg.DataPath}");

            IStrategy strategy = cfg.Strategy switch
            {
                "mac" => new MovingAverageCrossStrategy(cfg.Fast, cfg.Slow),
                _ => throw new ArgumentException($"Unknown strategy '{cfg.Strategy}'. Try 'mac' (moving average cross).")
            };

            var engine = new BacktestEngine(cfg.Symbol ?? "SYMB", new ExecutionSimulator(cfg.Slippage));
            var (portfolio, trades) = engine.Run(bars, strategy, cfg.Cash);

            var eq = new EquityCurveReporter();
            var reportDir = cfg.ReportDir ?? "reports";
            Directory.CreateDirectory(reportDir);
            var eqPath = Path.Combine(reportDir, "equity_curve.csv");
            var trPath = Path.Combine(reportDir, "trades.csv");
            eq.WriteCsv(eqPath, portfolio.EquityCurve);
            TradeReport.WriteCsv(trPath, trades);
            Logger.Info($"Wrote reports: {eqPath} and {trPath}");

            Console.WriteLine();
            Console.WriteLine("== SUMMARY ==");
            Console.WriteLine($"Bars: {bars.Count}");
            Console.WriteLine($"Trades: {trades.Count}");
            Console.WriteLine($"Start Equity: {cfg.Cash:C2}");
            Console.WriteLine($"End Equity:   {portfolio.EquityCurve.Last().Equity:C2}");
            Console.WriteLine($"Return:       {(portfolio.EquityCurve.Last().Equity / cfg.Cash - 1.0):P2}");
            Console.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            return 1;
        }
    }
}
