using Quant.Models;
using Quant.Analytics;

namespace Quant.Tests;

public class TwrIrrTests
{
    [Fact]
    public void TimeWeighted_LinkedTotal_ExcludesFlows()
    {
        var recs = new List<DailyRecord> {
            new(new DateOnly(2020,1,2), 1000, 0),
            new(new DateOnly(2020,1,3), 1100, 100),
            new(new DateOnly(2020,1,6), 1111, 0)
        };

        var (daily, linked) = PerformancePlus.TimeWeightedReturns(recs);
        Assert.Equal(2, daily.Count);
        Assert.InRange(daily[0], -1e-12, 1e-12);
        Assert.InRange(daily[1], 0.0099, 0.0101);
        Assert.InRange(linked, 0.0099, 0.0101);
    }

    [Fact]
    public void IRR_SimpleCase_ApproxTenPercent()
    {
        var flows = new List<CashFlow> {
            new(new DateOnly(2020,1,2), 1000)
        };
        var irr = PerformancePlus.IRR(flows, new DateOnly(2021,1,2), 1100);
        Assert.InRange(irr, 0.095, 0.105);
    }
}
