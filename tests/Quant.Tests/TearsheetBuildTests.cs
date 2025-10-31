using System;
using QuantFrameworks.Reporting;
using QuantFrameworks.Reporting.Tearsheet;
using Xunit;

namespace Quant.Tests.Reporting
{
    public class TearsheetBuildTests
    {
        [Fact]
        public void Build_From_Run_Computes_Summary()
        {
            // Don't assign NAV here; it's read-only and not needed by the tear sheet.
            var summary = new SummaryReport { Sharpe = 1.2m };

            var run = new RunReport
            {
                Start = new DateTime(2024,1,1),
                End = new DateTime(2024,1,10),
                StartingCash = 100000m,
                EndingNAV = 110000m,
                MaxDrawdown = 0.1m
            };
            run.DailyNav.Add((new DateTime(2024,1,1), 100000m));
            run.DailyNav.Add((new DateTime(2024,1,10), 110000m));

            var m = TearsheetFromRun.Build(summary, run, "Test Sheet");
            Assert.Equal("Test Sheet", m.Title);
            Assert.Equal(2, m.DailyNav.Count);
            Assert.True(m.Summary.ContainsKey("Total Return (%)"));
            Assert.Equal(10m, Math.Round(m.Summary["Total Return (%)"], 2)); // (110000/100000 -1)*100 = 10%
        }
    }
}
