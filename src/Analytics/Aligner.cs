namespace Quant.Analytics;

public static class Aligner
{
    public static (List<ReturnPoint> a, List<ReturnPoint> b) AlignByDate(
        IEnumerable<ReturnPoint> a, IEnumerable<ReturnPoint> b)
    {
        var da = a.ToDictionary(x => x.Date, x => x.Return);
        var db = b.ToDictionary(x => x.Date, x => x.Return);
        var common = da.Keys.Intersect(db.Keys).OrderBy(d => d).ToList();

        var outA = new List<ReturnPoint>(common.Count);
        var outB = new List<ReturnPoint>(common.Count);

        foreach (var d in common)
        {
            outA.Add(new ReturnPoint(d, da[d]));
            outB.Add(new ReturnPoint(d, db[d]));
        }
        return (outA, outB);
    }
}
