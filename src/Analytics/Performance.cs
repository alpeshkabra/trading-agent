namespace Quant.Analytics;

public static class Performance
{
    // Wealth index from simple returns: starts at 1.0, multiplies by (1+r)
    public static IReadOnlyList<double> CumulativeWealth(IEnumerable<double> returns)
    {
        var result = new List<double>();
        double w = 1.0;
        foreach (var r in returns)
        {
            w *= (1.0 + r);
            result.Add(w);
        }
        return result;
    }

    // Annualized volatility = sample stddev * sqrt(periodsPerYear)
    public static double AnnualizedVolatility(IEnumerable<double> returns, int periodsPerYear = 252)
    {
        var xs = returns.ToArray();
        if (xs.Length == 0) return double.NaN;

        double mean = xs.Average();
        double sumSq = 0.0;
        for (int i = 0; i < xs.Length; i++)
        {
            var d = xs[i] - mean;
            sumSq += d * d;
        }
        double variance = sumSq / Math.Max(1, xs.Length - 1);
        return Math.Sqrt(variance) * Math.Sqrt(periodsPerYear);
    }

    // Annualized Sharpe (rf ~ 0): (mean * periodsPerYear) / annVol
    public static double Sharpe(IEnumerable<double> returns, int periodsPerYear = 252)
    {
        var xs = returns.ToArray();
        if (xs.Length == 0) return double.NaN;

        var annVol = AnnualizedVolatility(xs, periodsPerYear);
        if (double.IsNaN(annVol) || annVol == 0) return double.NaN;

        var mean = xs.Average();
        var annMean = mean * periodsPerYear;
        return annMean / annVol;
    }

    // Max drawdown from wealth; returns (drawdown as +fraction, peakIdx, troughIdx)
    public static (double mdd, int peak, int trough) MaxDrawdown(IReadOnlyList<double> wealth)
    {
        if (wealth.Count == 0) return (double.NaN, -1, -1);

        double peakVal = wealth[0];
        int peakIdx = 0;
        double maxDD = 0.0;
        int ddStart = 0, ddEnd = 0;

        for (int i = 1; i < wealth.Count; i++)
        {
            if (wealth[i] > peakVal)
            {
                peakVal = wealth[i];
                peakIdx = i;
            }
            var dd = 1.0 - wealth[i] / peakVal;
            if (dd > maxDD)
            {
                maxDD = dd;
                ddStart = peakIdx;
                ddEnd = i;
            }
        }
        return (maxDD, ddStart, ddEnd);
    }

    // Total return = product(1+r) - 1
    public static double TotalReturn(IEnumerable<double> returns)
    {
        double w = 1.0;
        foreach (var r in returns) w *= (1.0 + r);
        return w - 1.0;
    }
}
