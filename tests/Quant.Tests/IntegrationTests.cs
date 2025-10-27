using Quant.Analytics;
using Quant.Reports;

namespace Quant.Tests;

public class IntegrationTests
{
    [Fact]
    public void Excess_Returns_And_CsvWrite_Work()
    {
        var spyPx = new List<PricePoint> {
            new(new DateOnly(2020,1,2), 100),
            new(new DateOnly(2020,1,3), 101),
            new(new DateOnly(2020,1,6), 100)
        };
        var stkPx = new List<PricePoint> {
            new(new DateOnly(2020,1,2), 100),
            new(new DateOnly(2020,1,3), 102),
            new(new DateOnly(2020,1,6), 99)
        };

        var spyR = Returns.Simple(spyPx).ToList();
        var stkR = Returns.Simple(stkPx).ToList();

        var (a, b) = Aligner.AlignByDate(spyR, stkR);
        Assert.Equal(2, a.Count);
        Assert.Equal(2, b.Count);

        var excess0 = b[0].Return - a[0].Return;
        Assert.InRange(excess0, 0.0099, 0.0101);

        var excess1 = b[1].Return - a[1].Return;
        Assert.InRange(excess1, -0.0196, -0.0194);

        var rows = new List<string[]>();
        rows.Add(new[] { "Date","SPY_Return","STK_Return","Excess" });
        for (int i = 0; i < a.Count; i++)
        {
            rows.Add(new[] {
                a[i].Date.ToString("yyyy-MM-dd"),
                a[i].Return.ToString("F6"),
                b[i].Return.ToString("F6"),
                (b[i].Return - a[i].Return).ToString("F6")
            });
        }

        var tmp = Path.Combine(Path.GetTempPath(), "compare_STK_vs_SPY.csv");
        try
        {
            CsvWriter.Write(tmp, rows);
            Assert.True(File.Exists(tmp));
            var lines = File.ReadAllLines(tmp);
            Assert.Equal(3, lines.Length);
        }
        finally { if (File.Exists(tmp)) File.Delete(tmp); }
    }
}
