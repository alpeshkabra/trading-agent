using Quant.Analytics;
using Quant.Models;

namespace Quant.Portfolio;

public static class WeightedPortfolio
{
    /// <summary>
    /// Build a daily rebalanced (to target weights) portfolio from multiple return series.
    /// Each asset: List&lt;ReturnPoint&gt; (Date, Return). Returns aligned intersection by date.
    /// Weights can be any real numbers; they will be linearly normalized to sum to 1.
    /// </summary>
    public static List<PortfolioPoint> Build(
        Dictionary<string, List<ReturnPoint>> assetReturns,
        Dictionary<string, double> weights)
    {
        if (assetReturns.Count == 0) return new List<PortfolioPoint>();
        if (weights.Count == 0) throw new ArgumentException("weights cannot be empty");

        // Keep only weights for assets we have, and ensure at least one survives
        var usable = weights.Where(kv => assetReturns.ContainsKey(kv.Key))
                            .ToDictionary(kv => kv.Key, kv => kv.Value);
        if (usable.Count == 0)
            throw new ArgumentException("no overlapping tickers between series and weights");

        // Normalize weights to sum to 1
        var sumW = usable.Values.Sum();
        if (Math.Abs(sumW) < 1e-12) throw new ArgumentException("weights sum to ~0; cannot normalize");
        var w = usable.ToDictionary(kv => kv.Key, kv => kv.Value / sumW);

        // Align all series by date (intersection)
        // 1) Build a map<date, map<ticker, return>>
        var allDates = assetReturns.Values
            .SelectMany(list => list.Select(p => p.Date))
            .GroupBy(d => d)
            .Select(g => g.Key)
            .ToHashSet();

        // Start with the dates from the first asset, then intersect with others
        var common = assetReturns.Values
            .Select(list => list.Select(p => p.Date).ToHashSet())
            .Aggregate((acc, next) => { acc.IntersectWith(next); return acc; })
            .OrderBy(d => d)
            .ToList();

        // Quick lookup per asset
        var dictPerAsset = new Dictionary<string, Dictionary<DateOnly, double>>();
        foreach (var (ticker, series) in assetReturns)
            dictPerAsset[ticker] = series.ToDictionary(p => p.Date, p => p.Return);

        // Build portfolio points (daily rebalanced to target weights)
        var outPts = new List<PortfolioPoint>(common.Count);
        double wealth = 1.0;

        foreach (var d in common)
        {
            // daily rebalanced portfolio return = sum (w_i * r_i)
            double portRet = 0.0;

            foreach (var (ticker, wi) in w)
            {
                if (!dictPerAsset.TryGetValue(ticker, out var rtMap) || !rtMap.TryGetValue(d, out var r))
                {
                    // Missing this date for that ticker -> skip; equivalently you could drop the day
                    // but since 'common' is intersection, this shouldn't happen.
                    continue;
                }
                portRet += wi * r;
            }

            wealth *= (1.0 + portRet);
            outPts.Add(new PortfolioPoint(d, portRet, wealth));
        }

        return outPts;
    }
}
