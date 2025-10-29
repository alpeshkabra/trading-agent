using Quant.Analytics;
using Quant.Models;

namespace Quant.Tests;

public class RiskMetricsTests
{
    [Fact]
    public void Volatility_ApproxSigma()
    {
        var xs = new List<double>();
        var rnd = new Random(7);
        // Build synthetic N(0, 0.02^2) via Box-Muller
        for (int i = 0; i < 1000; i++)
        {
            double u1 = 1.0 - rnd.NextDouble();
            double u2 = 1.0 - rnd.NextDouble();
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            xs.Add(0.02 * z);
        }
        var vol = RiskMetrics.Volatility(xs);
        Assert.InRange(vol, 0.018, 0.022);
    }

    [Fact]
    public void VaR_CVar_Sanity()
    {
        var xs = new double[] { -0.10, 0.00, 0.02, -0.03, 0.01 };
        var var20 = RiskMetrics.VaR(xs, 0.20);
        var cvar20 = RiskMetrics.CVar(xs, 0.20);
        Assert.True(var20 > 0);
        Assert.True(cvar20 >= var20 * 0.9);
    }

    [Fact]
    public void Rolling_Windows_Work()
    {
        var dates = new List<DateOnly> {
            new(2020,1,2), new(2020,1,3), new(2020,1,6), new(2020,1,7), new(2020,1,8)
        };
        var series = new List<Quant.Analytics.ReturnPoint>();
        for (int i = 0; i < dates.Count; i++)
            series.Add(new Quant.Analytics.ReturnPoint(dates[i], (i - 2) * 0.001));

        var snaps = RiskMetrics.Rolling(series, 3, 0.2);
        Assert.Equal(3, snaps.Count);
        foreach (var s in snaps)
        {
            Assert.True(double.IsFinite(s.Volatility));
            Assert.True(double.IsFinite(s.DownsideDev));
            Assert.True(double.IsFinite(s.VaR));
            Assert.True(double.IsFinite(s.CVar));
        }
    }
}
