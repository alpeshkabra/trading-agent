using System.Text.Json;
using Quant.Models;

namespace Quant.Reports;

public static class SummaryReporter
{
    public static void WriteCsv(string path, IEnumerable<PerformanceSummary> summaries)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        using var sw = new StreamWriter(path);
        sw.WriteLine("Label,Observations,TotalReturn,AnnualizedVol,Sharpe,MaxDrawdown");
        foreach (var s in summaries)
        {
            sw.WriteLine(string.Join(",", new[] {
                Quote(s.Label),
                s.Observations.ToString(),
                s.TotalReturn.ToString("F6"),
                s.AnnualizedVol.ToString("F6"),
                s.Sharpe.ToString("F6"),
                s.MaxDrawdown.ToString("F6")
            }));
        }
    }

    public static void WriteJson(string path, IEnumerable<PerformanceSummary> summaries)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        var opts = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(path, JsonSerializer.Serialize(summaries, opts));
    }

    private static string Quote(string s)
    {
        if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
        {
            var escaped = s.Replace("\"", "\"\"");
            return "\"" + escaped + "\"";
        }
        return s;
    }
}
