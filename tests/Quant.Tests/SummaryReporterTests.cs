using Quant.Analytics;
using Quant.Models;
using Quant.Reports;

namespace Quant.Tests;

public class SummaryReporterTests
{
    [Fact]
    public void WritesCsvAndJson_Files()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "summary_test");
        var csv = Path.Combine(tmp, "s.csv");
        var json = Path.Combine(tmp, "s.json");

        var summaries = new List<PerformanceSummary> {
            new PerformanceSummary { Label="SPY", Observations=2, TotalReturn=0.01, AnnualizedVol=0.2, Sharpe=1.0, MaxDrawdown=0.05 },
            new PerformanceSummary { Label="AAPL", Observations=2, TotalReturn=0.02, AnnualizedVol=0.25, Sharpe=1.2, MaxDrawdown=0.08 },
        };

        try
        {
            SummaryReporter.WriteCsv(csv, summaries);
            SummaryReporter.WriteJson(json, summaries);

            Assert.True(File.Exists(csv));
            Assert.True(File.Exists(json));

            var csvLines = File.ReadAllLines(csv);
            Assert.True(csvLines.Length >= 2);
            var jsonText = File.ReadAllText(json);
            Assert.Contains("\"Label\": \"SPY\"", jsonText);
        }
        finally
        {
            if (Directory.Exists(tmp))
            {
                foreach (var f in Directory.GetFiles(tmp)) File.Delete(f);
                Directory.Delete(tmp, true);
            }
        }
    }
}
