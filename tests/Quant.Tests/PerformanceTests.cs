using Quant.Analytics;
using Quant.Models;
using Quant.Reports;

namespace Quant.Tests;

public class PerformanceTests
{
    [Fact]
    public void Cumulative_TotalReturn_MaxDrawdown_Work()
    {
        var rets = new double[] { 0.05, -0.10 };
        var wealth = Performance.CumulativeWealth(rets);
        Assert.Equal(2, wealth.Count);
        Assert.InRange(wealth[0], 1.0499, 1.0501);
        Assert.InRange(wealth[1], 0.9449, 0.9451);

        var tr = Performance.TotalReturn(rets);
        Assert.InRange(tr, -0.0551, -0.0549);

        var (mdd, peak, trough) = Performance.MaxDrawdown(wealth);
        Assert.InRange(mdd, 0.0999, 0.1001);        
        Assert.Equal(0, peak);
        Assert.Equal(1, trough);
    }

    [Fact]
    public void Sharpe_And_AnnVol_Work()
    {
        var rets = Enumerable.Repeat(0.0001, 100).Select((r,i) => i%2==0 ? r : -r);
        var vol = Performance.AnnualizedVolatility(rets);
        Assert.True(double.IsFinite(vol));
        var sh = Performance.Sharpe(rets);
        Assert.True(double.IsNaN(sh) || double.IsFinite(sh));
    }
}
