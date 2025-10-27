using Quant.Analytics;

namespace Quant.Tests;

public class AlignerTests
{
    [Fact]
    public void AlignByDate_ReturnsIntersection_InOrder()
    {
        var a = new List<ReturnPoint> {
            new(new DateOnly(2020,1,3), 0.01),
            new(new DateOnly(2020,1,6), -0.02),
            new(new DateOnly(2020,1,7), 0.03),
        };
        var b = new List<ReturnPoint> {
            new(new DateOnly(2020,1,6), 0.004),
            new(new DateOnly(2020,1,7), 0.002),
            new(new DateOnly(2020,1,8), 0.001),
        };

        var (aa, bb) = Aligner.AlignByDate(a, b);

        Assert.Equal(2, aa.Count);
        Assert.Equal(2, bb.Count);
        Assert.Equal(new DateOnly(2020,1,6), aa[0].Date);
        Assert.Equal(new DateOnly(2020,1,7), aa[1].Date);
        Assert.Equal(0.004, bb[0].Return);
        Assert.Equal(0.002, bb[1].Return);
    }
}
