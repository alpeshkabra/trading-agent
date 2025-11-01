using Quant.Models;
using Quant.Analytics;
using Quant.Reports;
using Quant.Portfolio;
using QuantFrameworks.Signals;
using QuantFrameworks.DataCheck;
using QuantFrameworks.Corr;
using QuantFrameworks.Beta;

// New usings for backtest/optimize/report
using System.Globalization;
using System.Text.Json;
using QuantFrameworks.Backtest;
using QuantFrameworks.Optimize; // NEW: optimize feature
using PortfolioSummaryReporter = Quant.Reports.SummaryReporter; // alias to avoid ambiguity
using QFReporting = QuantFrameworks.Reporting;
using QuantFrameworks.Reporting;                 // for SummaryReporterWriter / RunReportWriter
using QuantFrameworks.Reporting.Tearsheet;       // for TearsheetFromRun / TearsheetWriter

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

                # Existing SPY vs stocks comparison (default)
                dotnet run -- --spy <path-to-SPY.csv> --stocks <CSV1,CSV2,...> [--from YYYY-MM-DD] [--to YYYY-MM-DD]
                            [--field Close] [--out reports]
                            [--portfolio TICKER=w,TICKER=w,...] [--portfolio-label NAME]

                # Backtest command (reads JSON config)
                dotnet run -- backtest --config <path-to-config.json>

                # NEW: Parameter Sweep & Walk-Forward Optimization
                dotnet run -- optimize --config examples/configs/optimize.json

                # NEW: Tear Sheet report (HTML/Markdown)
                dotnet run -- report --summary out/summary.csv --run out/run.json --out out/tearsheet.html --md out/tearsheet.md

                # NEW: RiskGuard - Pre-trade risk checks & position sizing
                dotnet run -- risk-check --orders ./data/orders.csv --config ./config/risk.json [--prices ./data/prices.csv] --out ./out

                CSV format (OHLCV daily, header required):
                Date,Open,High,Low,Close,Volume

                Example (SPY compare):
                dotnet run -- --spy data/SPY.csv --stocks data/AAPL.csv,data/MSFT.csv --from 2018-01-01 --out reports

                Example (Single-asset backtest):
                dotnet run -- backtest --config examples/configs/sma.json

                Example (Multi-asset backtest with sizing & exposure):
                dotnet run -- backtest --config examples/configs/multi.json

                Example (Optimize grid + WFO):
                dotnet run -- optimize --config examples/configs/optimize.json

                Example (RiskGuard):
                dotnet run -- risk-check --orders ./data/orders.csv --config ./config/risk.json --out ./out

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

                # Position sizing & exposure
                SizingMode: ""FixedDollar"" | ""PercentNav""
                DollarsPerTrade: 10000
                PercentNavPerTrade: 0.05
                Lot Size: 10
                MaxGrossExposurePct: 1.5

                Outputs:
                - out/summary.csv
                - out/daily_nav.csv
                - out/run.json

                Optimize outputs (when using `optimize`):
                - out/optimize/sweep_results.csv / .json
                - out/optimize/topN.csv
                - out/optimize/wfo_results.csv / .json
                ");
    }

    public static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            Help();
            return 0;
        }

        // Subcommand: "optimize"
        if (string.Equals(args[0], "optimize", StringComparison.OrdinalIgnoreCase))
            return RunOptimize(args);

        // Subcommand: "backtest"
        if (string.Equals(args[0], "backtest", StringComparison.OrdinalIgnoreCase))
            return RunBacktest(args);

        // Subcommand: "report"
        if (string.Equals(args[0], "report", StringComparison.OrdinalIgnoreCase))
            return RunReport(args);

        // ✅ Subcommand: "signal" (explicit OR permissive by flags)
        if (string.Equals(args[0], "signal", StringComparison.OrdinalIgnoreCase) ||
            ((args.Contains("--data") && args.Contains("--out")) &&
            (args.Contains("--sma-fast") || args.Contains("--sma-slow") ||
            args.Contains("--rsi") || args.Contains("--bb") ||
            args.Contains("--macd-fast") || args.Contains("--macd-slow") || args.Contains("--macd-signal"))))
        {
            return RunSignal(args);
        }

        // ✅ Subcommand: "data-check" (EXPLICIT ONLY — prevents hijacking `signal`)
        if (string.Equals(args[0], "data-check", StringComparison.OrdinalIgnoreCase))
        {
            return RunDataCheck(args);
        }

        // Subcommand: "risk-check" (no System.CommandLine dependency)
        if (string.Equals(args[0], "risk-check", StringComparison.OrdinalIgnoreCase) ||
            (args.Contains("--orders") && args.Contains("--config") && args.Contains("--out")))
        {
            return QuantFrameworks.Risk.RiskEntry.Run(args);
        }

        if (string.Equals(args[0], "corr", StringComparison.OrdinalIgnoreCase))
            return RunCorr(args);

        // Subcommand: "beta"
        if (string.Equals(args[0], "beta", StringComparison.OrdinalIgnoreCase))
            return RunBeta(args);
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

                // Use the alias to avoid ambiguity
                PortfolioSummaryReporter.WriteCsv(Path.Combine(outDir, $"summary_portfolio_{portfolioLabel}.csv"), summary);
                PortfolioSummaryReporter.WriteJson(Path.Combine(outDir, $"summary_portfolio_{portfolioLabel}.json"), summary);
            }
        }

        return 0;
    }

    // --------------- Backtester ---------------
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

            var runner = new QuantFrameworks.Backtest.MultiAssetBacktestRunner(cfg);
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

    // --------------- Optimize ---------------
    private static int RunOptimize(string[] args)
    {
        string? cfgPath = GetArg(args, "config");
        if (string.IsNullOrWhiteSpace(cfgPath))
        {
            Console.Error.WriteLine("ERROR: optimize requires --config <path-to-optimize.json>");
            return 2;
        }

        try
        {
            cfgPath = ResolveConfigPath(cfgPath);
            var json = File.ReadAllText(cfgPath);
            var cfg = System.Text.Json.JsonSerializer.Deserialize<QuantFrameworks.Optimize.OptimizerConfig>(
                json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Invalid optimize config JSON.");

            Console.WriteLine("Optimize config:");
            Console.WriteLine($"  Params        : {string.Join(", ", cfg.Parameters.Select(p => p.Name))}");
            Console.WriteLine($"  Folds (WFO)   : {cfg.Wfo?.KFolds ?? 0}");
            Console.WriteLine($"  Metric        : {cfg.TargetMetric}");
            Console.WriteLine($"  Max Parallel  : {cfg.MaxDegreeOfParallelism}");

            var sweep = new QuantFrameworks.Optimize.SweepRunner(cfg);
            var sweepResult = sweep.Run();

            QuantFrameworks.Optimize.OptimizeCsvWriter.WriteSweep(sweepResult, cfg.OutputDir);
            QuantFrameworks.Optimize.OptimizeJsonWriter.WriteSweep(sweepResult, cfg.OutputDir);
            QuantFrameworks.Optimize.OptimizeCsvWriter.WriteTopN(sweepResult, cfg.OutputDir, cfg.TopN);

            if (cfg.Wfo is not null && cfg.Wfo.KFolds > 0)
            {
                var wfo = new QuantFrameworks.Optimize.WfoRunner(cfg);
                var wfoResult = wfo.Run();
                QuantFrameworks.Optimize.OptimizeCsvWriter.WriteWfo(wfoResult, cfg.OutputDir);
                QuantFrameworks.Optimize.OptimizeJsonWriter.WriteWfo(wfoResult, cfg.OutputDir);
            }

            Console.WriteLine($"\nSaved reports under: {Path.GetFullPath(cfg.OutputDir)}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Optimize failed: " + Message(ex));
            return 1;
        }

        static string Message(Exception ex) => ex.Message;
    }

    // --------------- Tear Sheet report ---------------
    private static int RunReport(string[] args)
    {
        string? summaryPath = GetArg(args, "summary");
        string? runPath = GetArg(args, "run");
        string? outHtml = GetArg(args, "out");
        string? outMd = GetArg(args, "md");
        string title = GetArg(args, "title", "Backtest Tear Sheet") ?? "Backtest Tear Sheet";

        if (string.IsNullOrWhiteSpace(summaryPath) || string.IsNullOrWhiteSpace(runPath) || string.IsNullOrWhiteSpace(outHtml))
        {
            Console.Error.WriteLine("ERROR: report requires --summary <path> --run <path> --out <out.html>");
            return 2;
        }

        try
        {
            var run     = ReportLoaders.LoadRunJson(runPath);
            var summary = ReportLoaders.LoadSummaryFlexible(summaryPath, run.EndingNAV);

            var model = TearsheetFromRun.Build(summary, run, title);
            TearsheetWriter.WriteHtml(model, outHtml);
            if (!string.IsNullOrWhiteSpace(outMd))
                TearsheetWriter.WriteMarkdown(model, outMd);

            Console.WriteLine($"Saved tear sheet: {Path.GetFullPath(outHtml)}");
            if (!string.IsNullOrWhiteSpace(outMd))
                Console.WriteLine($"Saved markdown  : {Path.GetFullPath(outMd)}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Report failed: " + ex.Message);
            return 1;
        }
    }

    // -------- Helpers --------
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

    private static int RunDataCheck(string[] args)
    {
        string? data = GetArg(args, "data");
        string outDir = GetArg(args, "out", "out/dqx")!;
        int maxGapDays = int.TryParse(GetArg(args, "max-gap-days"), out var g) ? g : 3;
        double maxAbsRet = double.TryParse(GetArg(args, "max-abs-return", "0.25"),
                                        System.Globalization.NumberStyles.Any,
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        out var r) ? r : 0.25;
        long minVol = long.TryParse(GetArg(args, "min-volume"), out var mv) ? mv : 0;
        string failOn = GetArg(args, "fail-on", "none")!.ToLowerInvariant(); // none|any|outliers|gaps|duplicates

        if (string.IsNullOrWhiteSpace(data))
        {
            Console.Error.WriteLine("ERROR: data-check requires --data <path-to-ohlcv.csv>");
            return 2;
        }

        try
        {
            Directory.CreateDirectory(outDir);
            var cfg = new QuantFrameworks.DataCheck.DqxConfig
            {
                MaxGapDays = maxGapDays,
                MaxAbsReturn = maxAbsRet,
                MinVolume = minVol,
                FailOn = failOn
            };
            var summary = QuantFrameworks.DataCheck.DataCheckRunner.Run(data, outDir, cfg);
            Console.WriteLine($"DQX: gaps={summary.Gaps} dup={summary.Duplicates} outliers={summary.Outliers} badRows={summary.BadRows}");

            // map to exit codes if requested
            bool shouldFail =
                (failOn == "any" && summary.TotalIssues > 0) ||
                (failOn == "outliers" && summary.Outliers > 0) ||
                (failOn == "gaps" && summary.Gaps > 0) ||
                (failOn == "duplicates" && summary.Duplicates > 0);

            return shouldFail ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Data check failed: " + ex.Message);
            return 1;
        }
    }
    
    private static int RunSignal(string[] args)
    {
        string? data = GetArg(args, "data");
        string outDir = GetArg(args, "out", "out/sig")!;
        int smaFast = int.TryParse(GetArg(args, "sma-fast"), out var sf) ? sf : 0;
        int smaSlow = int.TryParse(GetArg(args, "sma-slow"), out var ss) ? ss : 0;
        int rsi = int.TryParse(GetArg(args, "rsi"), out var rp) ? rp : 0;
        double? rsiBuy = double.TryParse(GetArg(args, "rsi-buy"), NumberStyles.Any, CultureInfo.InvariantCulture, out var rb) ? rb : null;
        double? rsiSell = double.TryParse(GetArg(args, "rsi-sell"), NumberStyles.Any, CultureInfo.InvariantCulture, out var rs) ? rs : null;
        int bb = int.TryParse(GetArg(args, "bb"), out var bbp) ? bbp : 0;
        double bbStd = double.TryParse(GetArg(args, "bb-std", "2"), NumberStyles.Any, CultureInfo.InvariantCulture, out var s) ? s : 2.0;
        int macdFast = int.TryParse(GetArg(args, "macd-fast"), out var mf) ? mf : 0;
        int macdSlow = int.TryParse(GetArg(args, "macd-slow"), out var ms) ? ms : 0;
        int macdSignal = int.TryParse(GetArg(args, "macd-signal"), out var msi) ? msi : 0;

        if (string.IsNullOrWhiteSpace(data))
        {
            Console.Error.WriteLine("ERROR: signal requires --data <path-to-ohlcv.csv>");
            return 2;
        }

        try
        {
            Directory.CreateDirectory(outDir);
            var cfg = new QuantFrameworks.Signals.SignalConfig
            {
                SmaFast = smaFast,
                SmaSlow = smaSlow,
                Rsi = rsi,
                RsiBuy = rsiBuy,
                RsiSell = rsiSell,
                Bb = bb,
                BbStd = bbStd,
                MacdFast = macdFast,
                MacdSlow = macdSlow,
                MacdSignal = macdSignal
            };

            SignalRunner.Run(data, outDir, cfg);
            Console.WriteLine($"Saved indicators & signals to: {Path.GetFullPath(outDir)}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Signal generation failed: " + ex.Message);
            return 1;
        }
    }

    private static int RunCorr(string[] args)
    {
        string? symSpec = GetArg(args, "symbols");
        string outDir = GetArg(args, "out", "out/corr")!;
        int window = int.TryParse(GetArg(args, "window"), out var w) ? Math.Max(2, w) : 20;

        if (string.IsNullOrWhiteSpace(symSpec))
        {
            Console.Error.WriteLine("ERROR: corr requires --symbols \"TICK=path,TICK=path,...\"");
            return 2;
        }

        try
        {
            Directory.CreateDirectory(outDir);
            var cfg = new CorrConfig { Window = window, OutputDir = outDir };
            var pairs = CorrCli.ParseSymbols(symSpec); // Dictionary<string,string>
            CorrRunner.Run(pairs, cfg);
            Console.WriteLine($"Saved rolling correlation report under: {Path.GetFullPath(outDir)}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("corr failed: " + ex.Message);
            return 1;
        }
    }

    // --------------- Rolling Beta & Alpha ---------------
    private static int RunBeta(string[] args)
    {
        string? symSpec = GetArg(args, "symbols");
        string? bench = GetArg(args, "benchmark");
        string outDir = GetArg(args, "out", "out/beta")!;
        int window = int.TryParse(GetArg(args, "window"), out var w) ? Math.Max(2, w) : 60;

        if (string.IsNullOrWhiteSpace(symSpec) || string.IsNullOrWhiteSpace(bench))
        {
            Console.Error.WriteLine("ERROR: beta requires --symbols \"TICK=path,...\" and --benchmark <csv>");
            return 2;
        }

        try
        {
            Directory.CreateDirectory(outDir);
            var cfg = new BetaConfig { Window = window, OutputDir = outDir };
            var pairs = BetaCli.ParseSymbols(symSpec);
            BetaRunner.Run(pairs, bench, cfg);
            Console.WriteLine($"Saved beta/alpha reports under: {Path.GetFullPath(outDir)}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("beta failed: " + ex.Message);
            return 1;
        }
    }


}
