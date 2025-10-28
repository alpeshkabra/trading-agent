using Quant.Analytics;
using Quant.Models;
using Quant.Portfolio;

namespace Quant.Tests;

public class WeightedPortfolioTests
{
    [Fact]
    public void Build_NormalizesWeights_AlignsIntersection_ComputesWealth()
    {
        // Asset A prices -> returns
        var aPx = new List<PricePoint> {
            new(new DateOnly(2020,1,2), 100),
            new(new DateOnly(2020,1,3), 101),   // +1.0%
            new(new DateOnly(2020,1,6), 100),   // -0.9901%
            new(new DateOnly(2020,1,7), 102)    // +2.0%
        };
        var aR = Returns.Simple(aPx).ToList();   // dates: 1/3, 1/6, 1/7

        // Asset B prices -> returns (missing the middle day to test alignment)
        var bPx = new List<PricePoint> {
            new(new DateOnly(2020,1,2), 200),
            new(new DateOnly(2020,1,3), 202),   // +1.0%
            new(new DateOnly(2020,1,7), 206.04) // about +2.0% from previous (202)
        };
        var bR = Returns.Simple(bPx).ToList();   // dates: 1/3, 1/7

        var series = new Dictionary<string, List<ReturnPoint>>
        {
            ["A"] = aR,
            ["B"] = bR
        };

        // Weights won't sum to 1; expect normalization:
        var weights = new Dictionary<string, double> { ["A"] = 2.0, ["B"] = 1.0 }; // normalize -> A 0.666..., B 0.333...

        var pts = WeightedPortfolio.Build(series, weights);

        // Intersection of dates across A and B: { 1/3, 1/7 } (1/6 dropped)
        Assert.Equal(2, pts.Count);
        Assert.Equal(new DateOnly(2020,1,3), pts[0].Date);
        Assert.Equal(new DateOnly(2020,1,7), pts[1].Date);

        // On 1/3: rA = +1.0%, rB = +1.0% => port = 0.666*1% + 0.333*1% = 1.0%
        Assert.InRange(pts[0].Return, 0.0099, 0.0101);
        Assert.InRange(pts[0].Wealth, 1.0099, 1.0101);

        // On 1/7 (relative to previous dates in intersection):
        // rA on 1/7 vs 1/6 = +2.0%; rB on 1/7 vs 1/3 = +2.0% => port 2.0%, wealth ~ 1.010 * 1.02
        Assert.InRange(pts[1].Return, 0.0199, 0.0201);
        Assert.InRange(pts[1].Wealth, 1.0300, 1.0303);
    }

    [Fact]
    public void Build_Throws_When_NoOverlapOrZeroWeights()
    {
        var series = new Dictionary<string, List<ReturnPoint>>
        {
            ["X"] = new List<ReturnPoint> { new(new DateOnly(2020,1,3), 0.01) }
        };

        // zero-sum weights
        Assert.Throws<ArgumentException>(() =>
            WeightedPortfolio.Build(series, new Dictionary<string, double> { ["X"] = 0, ["Y"] = 0 }));

        // no overlapping tickers
        Assert.Throws<ArgumentException>(() =>
            WeightedPortfolio.Build(series, new Dictionary<string, double> { ["Z"] = 1.0 }));
    }
}
