using Quant.Models;
using Quant.Ledger;

namespace Quant.Tests;

public class PnlEngineTests
{
    [Fact]
    public void DailySeries_AppliesTrades_ValuesPortfolio()
    {
        var prices = new Dictionary<string, Dictionary<DateOnly, double>>
        {
            ["A"] = new() {
                [new DateOnly(2020,1,2)] = 100,
                [new DateOnly(2020,1,3)] = 101,
                [new DateOnly(2020,1,6)] = 102
            }
        };

        var trades = new List<Trade> {
            new(new DateOnly(2020,1,2), "A", 5, 100, 0)
        };

        var recs = PnlEngine.BuildDailySeries(trades, prices, initialCash: 1000, cashFlows: null);
        Assert.Equal(3, recs.Count);
        Assert.Equal(1000, recs[0].Value, 5);
        Assert.Equal(1005, recs[1].Value, 5);
        Assert.Equal(1010, recs[2].Value, 5);
    }

    [Fact]
    public void DailySeries_MergesCashFlows()
    {
        var prices = new Dictionary<string, Dictionary<DateOnly, double>>
        {
            ["A"] = new() {
                [new DateOnly(2020,1,2)] = 100,
                [new DateOnly(2020,1,3)] = 100
            }
        };

        var trades = new List<Trade>();
        var cfs = new List<CashFlow> {
            new(new DateOnly(2020,1,3), 200)
        };

        var recs = PnlEngine.BuildDailySeries(trades, prices, 1000, cfs);
        Assert.Equal(2, recs.Count);
        Assert.Equal(0, recs[0].ExternalFlow, 6);
        Assert.Equal(200, recs[1].ExternalFlow, 6);
        Assert.Equal(1200, recs[1].Value, 6);
    }
}
