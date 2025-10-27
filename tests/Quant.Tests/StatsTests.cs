using Quant.Analytics;

namespace Quant.Tests;

public class StatsTests
{
    [Fact]
    public void Corr_One_ForIdentical()
    {
        var a = new double[] { 0.01, -0.02, 0.03, 0.01 };
        var b = new double[] { 0.01, -0.02, 0.03, 0.01 };
        var rho = Stats.Correlation(a, b);
        Assert.True(double.IsFinite(rho));
        Assert.InRange(rho, 0.9999, 1.0001);
    }

    [Fact]
    public void Corr_MinusOne_ForNegated()
    {
        var a = new double[] { 0.01, -0.02, 0.03, 0.01 };
        var b = a.Select(x => -x);
        var rho = Stats.Correlation(a, b);
        Assert.InRange(rho, -1.0001, -0.9999);
    }

    [Fact]
    public void Corr_NaN_ForUnequalLengths()
    {
        var a = new double[] { 0.01, 0.02 };
        var b = new double[] { 0.01 };
        var rho = Stats.Correlation(a, b);
        Assert.True(double.IsNaN(rho));
    }
}
