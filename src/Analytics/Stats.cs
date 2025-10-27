namespace Quant.Analytics;

public static class Stats
{
    public static double Correlation(IEnumerable<double> xs, IEnumerable<double> ys)
    {
        var x = xs.ToArray();
        var y = ys.ToArray();
        if (x.Length != y.Length || x.Length == 0) return double.NaN;

        double meanX = x.Average();
        double meanY = y.Average();

        double num = 0, denX = 0, denY = 0;
        for (int i = 0; i < x.Length; i++)
        {
            var dx = x[i] - meanX;
            var dy = y[i] - meanY;
            num += dx * dy;
            denX += dx * dx;
            denY += dy * dy;
        }
        var den = Math.Sqrt(denX * denY);
        if (den == 0) return double.NaN;
        return num / den;
    }
}
