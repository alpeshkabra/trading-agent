using System.Collections.Generic;

namespace QuantFrameworks.Reporting
{
    public static class PerformanceSeries
    {
        public static List<decimal> WealthFromNAV(List<decimal> nav)
        {
            var res = new List<decimal>(nav.Count);
            if (nav.Count == 0) return res;
            var baseNav = nav[0] == 0 ? 1m : nav[0];
            foreach (var n in nav) res.Add(baseNav == 0 ? 1m : (n / baseNav));
            return res;
        }

        public static (decimal maxDrawdown, int peakIndex, int troughIndex) MaxDrawdown(List<decimal> wealth)
        {
            decimal peak = 0m, maxDD = 0m;
            int pIdx = 0, tIdx = 0;
            for (int i = 0; i < wealth.Count; i++)
            {
                var w = wealth[i];
                if (w > peak) { peak = w; pIdx = i; }
                var dd = peak == 0 ? 0 : (peak - w) / peak;
                if (dd > maxDD) { maxDD = dd; tIdx = i; }
            }
            return (maxDD, pIdx, tIdx);
        }
    }
}
