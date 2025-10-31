using System;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace QuantFrameworks.Reporting
{
    public static class ReportLoaders
    {
        public static RunReport LoadRunJson(string path)
        {
            var json = File.ReadAllText(path);
            var run = JsonSerializer.Deserialize<RunReport>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (run is null) throw new InvalidOperationException($"Could not parse RunReport JSON: {path}");
            return run;
        }

        /// <summary>
        /// Loads SummaryReport from JSON or CSV.
        /// - JSON: deserializes strongly typed SummaryReport (no property assignments needed).
        /// - CSV: returns a default SummaryReport (read-only props can’t be set). We still parse NAV/Sharpe
        ///        to inform the user via console, but we don't assign to the model.
        /// </summary>
        public static SummaryReport LoadSummaryFlexible(string path, decimal fallbackNav)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            if (ext == ".json")
            {
                var json = File.ReadAllText(path);
                var s = JsonSerializer.Deserialize<SummaryReport>(
                    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (s is not null) return s;
                throw new InvalidOperationException($"Could not parse SummaryReport JSON: {path}");
            }

            // CSV best-effort: parse a couple of fields for logging only (no assignments)
            if (File.Exists(path))
            {
                try
                {
                    var lines = File.ReadAllLines(path);
                    if (lines.Length >= 2)
                    {
                        var headers = lines[0].Split(',', StringSplitOptions.TrimEntries);
                        var values  = lines[1].Split(',', StringSplitOptions.TrimEntries);

                        int idx(string name)
                            => Array.FindIndex(headers, h => string.Equals(h, name, StringComparison.OrdinalIgnoreCase));

                        decimal parse(string name, decimal def = 0m)
                        {
                            var i = idx(name);
                            if (i >= 0 && i < values.Length &&
                                decimal.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                                return v;
                            return def;
                        }

                        var csvNav    = parse("NAV", parse("EndingNAV", fallbackNav));
                        var csvSharpe = parse("Sharpe", 0m);

                        Console.WriteLine($"[report] CSV summary parsed: NAV≈{csvNav}, Sharpe≈{csvSharpe}");
                    }
                }
                catch
                {
                    // ignore CSV parse errors; we'll just return a default model
                }
            }

            // Return a default SummaryReport; Tearsheet uses Sharpe only, which will be 0 if unknown.
            return new SummaryReport();
        }
    }
}
