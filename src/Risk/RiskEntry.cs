using System;
using System.Collections.Generic;
using System.IO;

namespace QuantFrameworks.Risk
{
    // Lightweight CLI parser: no external packages.
    public static class RiskEntry
    {
        // BEFORE: public static int Main(string[] args)
        public static int Run(string[] args)
            {
                try
                {
                    var tokens = new List<string>(args ?? Array.Empty<string>());
                    if (tokens.Count > 0 && IsSubcommand(tokens[0])) tokens.RemoveAt(0);

                    var opts = ParseFlags(tokens);

                    if (!opts.TryGetValue("--orders", out var orders) ||
                        !opts.TryGetValue("--config", out var config) ||
                        !opts.TryGetValue("--out",    out var outDir))
                    {
                        PrintUsage();
                        return 2;
                    }

                    opts.TryGetValue("--prices", out var prices);

                    if (!File.Exists(orders)) throw new FileNotFoundException("Orders CSV not found", orders);
                    if (!File.Exists(config)) throw new FileNotFoundException("Config JSON not found", config);
                    if (prices is not null && !File.Exists(prices))
                        throw new FileNotFoundException("Prices CSV not found", prices);

                    RiskRunner.Run(orders, config, prices, outDir);
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[risk-check] {ex.GetType().Name}: {ex.Message}");
                    return 1;
                }
            }


        private static bool IsSubcommand(string s)
            => string.Equals(s, "risk-check", StringComparison.OrdinalIgnoreCase);

        private static Dictionary<string,string> ParseFlags(List<string> tokens)
        {
            var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < tokens.Count; i++)
            {
                var t = tokens[i];
                if (!t.StartsWith("--", StringComparison.Ordinal)) continue;

                // Support both "--flag value" and "--flag=value"
                string key = t;
                string? value = null;

                var eq = t.IndexOf('=');
                if (eq > 2)
                {
                    key = t.Substring(0, eq);
                    value = t.Substring(eq + 1);
                }
                else
                {
                    // collect one or more tokens as the value until the next flag
                    var parts = new List<string>();
                    int j = i + 1;
                    while (j < tokens.Count && !tokens[j].StartsWith("--", StringComparison.Ordinal))
                    {
                        parts.Add(tokens[j]);
                        j++;
                    }
                    if (parts.Count > 0) value = string.Join(' ', parts);
                    i = j - 1; // advance
                }

                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Trim optional surrounding quotes
                    if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
                        value = value.Substring(1, value.Length - 2);
                    dict[key] = value;
                }
            }

            return dict;
        }


        private static void PrintUsage()
        {
            Console.Error.WriteLine("RiskGuard usage:");
            Console.Error.WriteLine("  dotnet run -- risk-check --orders <orders.csv> --config <risk.json> [--prices <prices.csv>] --out <dir>");
            Console.Error.WriteLine("  dotnet run -- --orders <orders.csv> --config <risk.json> [--prices <prices.csv>] --out <dir>");
        }
    }
}
