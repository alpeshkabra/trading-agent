using Quant.Models;

namespace Quant.Analytics;

public static class RiskMetrics
{
    public static double Volatility(IEnumerable<double> returns)
    {
        var xs = returns.ToArray();
        if (xs.Length < 2) return double.NaN;
        double mean = xs.Average();
        double ss = 0.0;
        for (int i = 0; i < xs.Length; i++)
        {
            var d = xs[i] - mean;
            ss += d * d;
        }
        return Math.Sqrt(ss / (xs.Length - 1));
    }

    public static double DownsideDeviation(IEnumerable<double> returns, double mar = 0.0)
    {
        var xs = returns.ToArray();
        if (xs.Length == 0) return double.NaN;
        double ss = 0.0;
        int n = 0;
        for (int i = 0; i < xs.Length; i++)
        {
            var diff = xs[i] - mar;
            if (diff < 0)
            {
                ss += diff * diff;
                n++;
            }
        }
        if (n == 0) return 0.0;
        return Math.Sqrt(ss / xs.Length);
    }

    public static double VaR(IEnumerable<double> returns, double alpha = 0.05)
    {
        var xs = returns.ToArray();
        if (xs.Length == 0) return double.NaN;
        var q = Quantile(xs, alpha);
        return Math.Max(0.0, -q);
    }

    public static double CVar(IEnumerable<double> returns, double alpha = 0.05)
    {
        var xs = returns.ToArray();
        if (xs.Length == 0) return double.NaN;
        var q = Quantile(xs, alpha);
        var tail = xs.Where(v => v <= q).ToArray();
        if (tail.Length == 0) return 0.0;
        return Math.Max(0.0, -tail.Average());
    }

    public static List<RiskSnapshot> Rolling(IReadOnlyList<Quant.Analytics.ReturnPoint> series, int window, double alpha = 0.05)
    {
        var outList = new List<RiskSnapshot>();
        if (series.Count < window || window <= 1) return outList;

        for (int i = window - 1; i < series.Count; i++)
        {
            var seg = new double[window];
            for (int k = 0; k < window; k++) seg[k] = series[i - window + 1 + k].Return;

            var vol = Volatility(seg);
            var dd  = DownsideDeviation(seg);
            var varv = VaR(seg, alpha);
            var cvar = CVar(seg, alpha);

            outList.Add(new RiskSnapshot(series[i].Date, vol, dd, varv, cvar));
        }
        return outList;
    }

    private static double Quantile(double[] values, double alpha)
    {
        var xs = (double[])values.Clone();
        Array.Sort(xs);
        if (alpha <= 0) return xs[0];
        if (alpha >= 1) return xs[^1];
        double pos = alpha * (xs.Length - 1);
        int lo = (int)Math.Floor(pos);
        int hi = (int)Math.Ceiling(pos);
        if (lo == hi) return xs[lo];
        double w = pos - lo;
        return xs[lo] * (1 - w) + xs[hi] * w;
    }
}
