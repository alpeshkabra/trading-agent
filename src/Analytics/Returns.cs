namespace Quant.Analytics;

public readonly record struct PricePoint(DateOnly Date, double Price);
public readonly record struct ReturnPoint(DateOnly Date, double Return);

public static class Returns
{
    public static IEnumerable<ReturnPoint> Simple(IReadOnlyList<PricePoint> px)
    {
        for (int i = 1; i < px.Count; i++)
        {
            var prev = px[i-1];
            var curr = px[i];
            if (curr.Date == prev.Date) continue;
            if (prev.Price == 0) continue;
            yield return new ReturnPoint(curr.Date, (curr.Price / prev.Price) - 1.0);
        }
    }

    public static IEnumerable<ReturnPoint> Log(IReadOnlyList<PricePoint> px)
    {
        for (int i = 1; i < px.Count; i++)
        {
            var prev = px[i-1];
            var curr = px[i];
            if (curr.Date == prev.Date) continue;
            if (prev.Price <= 0 || curr.Price <= 0) continue;
            yield return new ReturnPoint(curr.Date, Math.Log(curr.Price / prev.Price));
        }
    }
}
