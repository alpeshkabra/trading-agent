using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using QuantFrameworks.Backtest;
using QuantFrameworks.IO;
using QuantFrameworks.Reporting;

namespace TradingAgent
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: trading-agent backtest --config <path>");
                return 0;
            }

            if (args[0].Equals("backtest", StringComparison.OrdinalIgnoreCase))
            {
                string cfgPath = args.Length > 2 && args[1] == "--config" ? args[2] : "examples/configs/sma.json";
                var json = await File.ReadAllTextAsync(cfgPath);
                var cfg = JsonSerializer.Deserialize<BacktestConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

                Console.WriteLine($"Running backtest for {cfg.Symbol} ({cfg.Fast}/{cfg.Slow})");
                if (cfg.StopLossPct > 0 || cfg.TakeProfitPct > 0)
                {
                    Console.WriteLine($"  StopLossPct   : {cfg.StopLossPct:P}");
                    Console.WriteLine($"  TakeProfitPct : {cfg.TakeProfitPct:P}");
                }

                var runner = new BacktestRunner(cfg);
                var rpt = await runner.RunAsync();
                SummaryReporterWriter.WriteConsole(rpt);
                SummaryReporterWriter.WriteCsv(rpt, cfg.OutputPath);
                Console.WriteLine($"Saved: {Path.GetFullPath(cfg.OutputPath)}");
                return 0;
            }

            Console.WriteLine("Unknown command");
            return 1;
        }
    }
}
