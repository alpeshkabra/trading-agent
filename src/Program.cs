using Quant.Data;
using Quant.Models;
using Quant.Analytics;
using Quant.Reports;
using Quant.Portfolio;

// New usings for backtest MVP
using System.Text.Json;
using QuantFrameworks.Backtest;
using PortfolioSummaryReporter = Quant.Reports.SummaryReporter;
using QFReporting = QuantFrameworks.Reporting;

namespace Quant;

public static class Program
{
    private static void Help()
    {
    Console.WriteLine(@"
                QuantFrameworks - CSV reader + SPY vs stocks daily return comparison
                Also includes an end-to-end SMA backtest engine (single or multi-asset), with
                position sizing (Fixed-$ / %NAV), lot rounding and Max Gross Exposure guard.

                Usage:

                # Existing SPY vs stocks comparison
                dotnet run -- --spy <path-to-SPY.csv> --stocks <CSV1,CSV2,...> [--from YYYY-MM-DD] [--to YYYY-MM-DD]
                            [--field Close] [--out reports]
                            [--portfolio TICKER=w,TICKER=w,...] [--portfolio-label NAME]

                # New backtest command (reads JSON config)
                dotnet run -- backtest --config <path-to-config.json>

                CSV format (OHLCV daily, header required):
                Date,Open,High,Low,Close,Volume

                Example (SPY compare):
                dotnet run -- --spy data/SPY.csv --stocks data/AAPL.csv,data/MSFT.csv --from 2018-01-01 --out reports

                Example (Single-asset backtest):
                dotnet run -- backtest --config examples/configs/sma.json

                Example (Multi-asset backtest with sizing & exposure):
                dotnet run -- backtest --config examples/configs/multi.json

                Key backtest config fields:
                # Symbols & data
                Symbol: ""AAPL""
                DataPath: ""examples/data/AAPL.csv""
                Symbols: [""AAPL"", ""MSFT""]
                SymbolData: { ""AAPL"": ""examples/data/AAPL.csv"", ""MSFT"": ""examples/data/MSFT.csv"" }

                # Strategy
                Fast: 2
                Slow: 3
                StopLossPct: 0.02
                TakeProfitPct: 0.05

                # Costs & slippage
                CommissionPerOrder: 0.50
                PercentFee: 0.001
                MinFee: 0.10
                SlippageBps: 25

                # NEW: Position sizing & exposure
                SizingMode: ""FixedDollar"" | ""PercentNav""
                DollarsPerTrade: 10000
                PercentNavPerTrade: 0.05
                LotSize: 10
                MaxGrossExposurePct: 1.5

                Outputs:
                - out/summary.csv
                - out/daily_nav.csv
                - out/run.json
                ");
                }

    public static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            Help();
            return 0;
        }

        // Subcommand: "backtest"
        if (string.Equals(args[0], "backtest", StringComparison.OrdinalIgnoreCase))
        {
            return RunBacktest(args);
        }

        // Default: existing SPY vs stocks flow
        return RunSpyCompare(args);
    }

    // ----------------------
    // Existing SPY comparison
    // ----------------------
    private static int RunSpyCompare(string[] args)
    {
        string? spyPath = GetArg(args, "spy");
        string? stockList = GetArg(args, "stocks");
        string field = GetArg(args, "field", "Close")!;
        string outDir = GetArg(args, "out", "reports")!;
        DateOnly? from = ParseDate(GetArg(args, "from"));
        DateOnly? to = ParseDate(GetArg(args, "to"));
        string? portfolioSpec = GetArg(args, "portfolio");
        string portfolioLabel = GetArg(args, "portfolio-label", "combined")!;

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
        var assetReturns = new Dictionary<string, List<ReturnPoint>>();

        foreach (var path in stockPaths)
        {
            var bars = new CsvReader(path).ReadBars(from, to).ToList();
            if (bars.Count == 0) { Console.WriteLine($"WARN: no rows for {path}"); continue; }

            var ticker = InferTickerFromPath(path);
            var px = bars.Select(b => new PricePoint(b.Date, PickField(b, field))).ToList();
            var rets = Returns.Simple(px).ToList();
            assetReturns[ticker] = rets;
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
            Console.WriteLine($"Wrote {outPath} ({rows.Count - 1} rows).");

            var avgSpy = spyAligned.Average(p => p.Return);
            var avgStk = stockAligned.Average(p => p.Return);
            var corr = Stats.Correlation(spyAligned.Select(p => p.Return), stockAligned.Select(p => p.Return));
            Console.WriteLine($"Summary {ticker}: meanRet={avgStk:F6}, meanSPY={avgSpy:F6}, corr={corr:F4}");
        }

        if (!string.IsNullOrWhiteSpace(portfolioSpec) && assetReturns.Count > 0)
        {
            var weights = ParseWeights(portfolioSpec);
            if (weights.Count == 0)
            {
                Console.WriteLine("WARN: --portfolio provided but no valid weights were parsed.");
            }
            else
            {
                var pts = WeightedPortfolio.Build(assetReturns, weights);
                var portCsv = Path.Combine(outDir, $"portfolio_{portfolioLabel}.csv");
                PortfolioCsv.Write(portCsv, pts);
                Console.WriteLine($"Wrote {portCsv} ({pts.Count} rows).");

                var portR = pts.Select(p => p.Return).ToList();
                var wealth = Performance.CumulativeWealth(portR);
                var (mdd, _, _) = Performance.MaxDrawdown(wealth);
                var summary = new List<PerformanceSummary>
                {
                    new PerformanceSummary {
                        Label = $"PORT:{portfolioLabel}",
                        Observations = portR.Count,
                        TotalReturn = Performance.TotalReturn(portR),
                        AnnualizedVol = Performance.AnnualizedVolatility(portR),
                        Sharpe = Performance.Sharpe(portR),
                        MaxDrawdown = mdd
                    }
                };

                SummaryReporter.WriteCsv(Path.Combine(outDir, $"summary_portfolio_{portfolioLabel}.csv"), summary);
                SummaryReporter.WriteJson(Path.Combine(outDir, $"summary_portfolio_{portfolioLabel}.json"), summary);
            }
        }

        return 0;
    }

    // ---------------
    // New: Backtester
    // ---------------
    private static int RunBacktest(string[] args)
        {
            string? cfgPath = GetArg(args, "config");
            if (string.IsNullOrWhiteSpace(cfgPath))
            {
                Console.Error.WriteLine("ERROR: backtest requires --config <path-to-config.json>");
                return 2;
            }

            try
            {
                cfgPath = ResolveConfigPath(cfgPath);
                var json = File.ReadAllText(cfgPath);
                var cfg = System.Text.Json.JsonSerializer.Deserialize<QuantFrameworks.Backtest.BacktestConfig>(
                    json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Invalid config JSON.");

                Console.WriteLine("Backtest config:");
                if (!string.IsNullOrWhiteSpace(cfg.Symbol))
                    Console.WriteLine($"  Symbol        : {cfg.Symbol}");
                if (cfg.Symbols != null && cfg.Symbols.Count > 0)
                    Console.WriteLine("  Symbols       : " + string.Join(",", cfg.Symbols));
                Console.WriteLine($"  Fast/Slow SMA : {cfg.Fast}/{cfg.Slow}");
                if (cfg.StopLossPct > 0 || cfg.TakeProfitPct > 0)
                {
                    Console.WriteLine($"  StopLossPct   : {cfg.StopLossPct:P}");
                    Console.WriteLine($"  TakeProfitPct : {cfg.TakeProfitPct:P}");
                }
                if (cfg.SlippageBps > 0) Console.WriteLine($"  Slippage (bps): {cfg.SlippageBps}");
                if (cfg.CommissionPerOrder > 0 || cfg.PercentFee > 0 || cfg.MinFee > 0)
                {
                    Console.WriteLine($"  Commission/Fill : {cfg.CommissionPerOrder}");
                    Console.WriteLine($"  Percent Fee     : {cfg.PercentFee:P}");
                    if (cfg.MinFee > 0) Console.WriteLine($"  Min Fee         : {cfg.MinFee}");
                }

                if (!string.IsNullOrWhiteSpace(cfg.SizingMode))
                    Console.WriteLine($"  Sizing Mode   : {cfg.SizingMode}");
                if (cfg.DollarsPerTrade > 0)
                    Console.WriteLine($"  $/Trade       : {cfg.DollarsPerTrade}");
                if (cfg.PercentNavPerTrade > 0)
                    Console.WriteLine($"  %NAV/Trade    : {cfg.PercentNavPerTrade:P}");
                if (cfg.LotSize > 1)
                    Console.WriteLine($"  Lot Size      : {cfg.LotSize}");
                if (cfg.MaxGrossExposurePct > 0)
                    Console.WriteLine($"  Max Gross Exp.: {cfg.MaxGrossExposurePct:P0}");

                // Always use the multi-asset runner. It supports single-symbol too.
                var runner = new QuantFrameworks.Backtest.MultiAssetBacktestRunner(cfg);

                // Synchronous wait, so Program.Main stays int-returning.
                var result = runner.RunAsync().GetAwaiter().GetResult();
                var summary = result.summary;
                var run = result.run;

                QuantFrameworks.Reporting.SummaryReporterWriter.WriteConsole(summary);
                Directory.CreateDirectory(Path.GetDirectoryName(cfg.OutputPath) ?? ".");
                QuantFrameworks.Reporting.SummaryReporterWriter.WriteCsv(summary, cfg.OutputPath);

                Directory.CreateDirectory(Path.GetDirectoryName(cfg.DailyNavCsv) ?? ".");
                QuantFrameworks.Reporting.RunReportWriter.WriteDailyCsv(run, cfg.DailyNavCsv);

                Directory.CreateDirectory(Path.GetDirectoryName(cfg.RunJson) ?? ".");
                QuantFrameworks.Reporting.RunReportWriter.WriteJson(run, cfg.RunJson);

                Console.WriteLine($"\nSaved:");
                Console.WriteLine($"  {Path.GetFullPath(cfg.OutputPath)}");
                Console.WriteLine($"  {Path.GetFullPath(cfg.DailyNavCsv)}");
                Console.WriteLine($"  {Path.GetFullPath(cfg.RunJson)}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Backtest failed: " + ex.Message);
                return 1;
            }
        }


    // --------
    // Helpers
    // --------
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

    private static Dictionary<string, double> ParseWeights(string spec)
    {
        var dict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var kv = part.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (kv.Length != 2) continue;
            var k = kv[0].ToUpperInvariant();
            if (double.TryParse(kv[1], System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out var w))
            {
                dict[k] = w;
            }
        }
        return dict;
    }

    // More friendly config resolving (handles relative paths & current dir)
    private static string ResolveConfigPath(string cfgPath)
    {
        if (Path.IsPathRooted(cfgPath) && File.Exists(cfgPath)) return cfgPath;

        var candidates = new[]
        {
            cfgPath,
            Path.GetFullPath(cfgPath),
            Path.Combine(AppContext.BaseDirectory, cfgPath),
            Path.Combine(Environment.CurrentDirectory, cfgPath),
        };

        foreach (var c in candidates)
            if (File.Exists(c)) return c;

        throw new FileNotFoundException(
            $"Config not found: {cfgPath}\n" +
            $"Checked:\n  - {string.Join("\n  - ", candidates)}\n" +
            $"Tip: create examples\\configs\\sma.json or pass --config <full path>."
        );
    }
}
