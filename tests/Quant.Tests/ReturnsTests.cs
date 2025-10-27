using Quant.Analytics;

namespace Quant.Tests;

public class ReturnsTests
{
    [Fact]
    public void Simple_Returns_Work()
    {
        var px = new List<PricePoint> {
            new(new DateOnly(2020,1,2), 100),
            new(new DateOnly(2020,1,3), 101),
            new(new DateOnly(2020,1,6), 100)
        };
        var rets = Returns.Simple(px).ToList();
        Assert.Equal(2, rets.Count);
        Assert.Equal(0.01, Math.Round(rets[0].Return, 4));
        Assert.Equal(-0.0099, Math.Round(rets[1].Return, 4));
    }

    [Fact]
    public void Log_Returns_Work()
    {
        var px = new List<PricePoint> {
            new(new DateOnly(2020,1,2), 100),
            new(new DateOnly(2020,1,3), 105)
        };
        var rets = Returns.Log(px).ToList();
        Assert.Single(rets);
        Assert.Equal(Math.Log(1.05), rets[0].Return, 6);
    }
}
