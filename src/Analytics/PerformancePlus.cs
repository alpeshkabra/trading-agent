using Quant.Models;

namespace Quant.Analytics;

public static class PerformancePlus
{
    public static (List<double> dailyR, double linkedTotal) TimeWeightedReturns(IReadOnlyList<DailyRecord> records)
    {
        var res = new List<double>();
        if (records.Count < 2) return (res, double.NaN);

        for (int i = 1; i < records.Count; i++)
        {
            var prev = records[i - 1];
            var curr = records[i];
            if (prev.Value == 0) { res.Add(double.NaN); continue; }
            var r = (curr.Value - (prev.Value + curr.ExternalFlow)) / prev.Value;
            res.Add(r);
        }

        double wealth = 1.0;
        foreach (var r in res)
            if (double.IsFinite(r)) wealth *= (1.0 + r);
        return (res, wealth - 1.0);
    }

    public static double IRR(IEnumerable<CashFlow> flows, DateOnly terminalDate, double terminalValue)
    {
        var list = flows.ToList();
        list.Sort((a,b) => a.Date.CompareTo(b.Date));
        list.Add(new CashFlow(terminalDate, -terminalValue));
        if (list.Count == 0) return double.NaN;

        var t0 = list[0].Date;
        var items = list.Select(cf => (days: (cf.Date.DayNumber - t0.DayNumber), amt: cf.Amount)).ToList();

        double lo = -0.999, hi = 10.0;

        double f(double r)
        {
            double s = 0.0;
            foreach (var it in items)
            {
                double df = Math.Pow(1.0 + r, it.days / 365.0);
                s += it.amt / df;
            }
            return s;
        }

        double flo = f(lo), fhi = f(hi);
        int guard = 0;
        while (flo * fhi > 0 && guard < 20)
        {
            lo = Math.Max(-0.999, lo - 1.0);
            hi = hi + 1.0;
            flo = f(lo); fhi = f(hi);
            guard += 1;
        }

        for (int i = 0; i < 100; i++)
        {
            double mid = (lo + hi) / 2.0;
            double fm = f(mid);
            if (Math.Abs(fm) < 1e-12) return mid;
            if (flo * fm <= 0) { hi = mid; fhi = fm; }
            else { lo = mid; flo = fm; }
        }
        return (lo + hi) / 2.0;
    }
}
