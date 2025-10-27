namespace Backtesting.Indicators;

public static class MovingAverage
{
    public static IEnumerable<double?> SMA(IEnumerable<double> source, int period)
    {
        if (period <= 0) throw new ArgumentOutOfRangeException(nameof(period));
        var q = new Queue<double>();
        double sum = 0;
        foreach (var v in source)
        {
            q.Enqueue(v);
            sum += v;
            if (q.Count > period) sum -= q.Dequeue();
            if (q.Count == period) yield return sum / period;
            else yield return null;
        }
    }
}
