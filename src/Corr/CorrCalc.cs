using Quant;

namespace QuantFrameworks.Corr
{
    public static class CorrCalc
    {
        // Load Close series per symbol, aligned by date intersection, oldest->newest
        public static (List<DateOnly> dates, Dictionary<string,double[]> rets)
            LoadAlignedReturns(Dictionary<string,string> symbolPaths)
        {
            // 1) Read all price series
            var px = new Dictionary<string, List<(DateOnly, double)>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in symbolPaths)
            {
                var list = new List<(DateOnly, double)>();
                foreach (var b in new CsvReader(kv.Value).ReadBars())
                {
                    if (b.Close > 0) list.Add((b.Date, b.Close));
                }
                list.Sort((a, b) => a.Item1.CompareTo(b.Item1));
                px[kv.Key] = list;
            }

            // 2) Build date intersection
            HashSet<DateOnly>? intersect = null;
            foreach (var lst in px.Values)
            {
                var dates = lst.Select(t => t.Item1).ToHashSet();
                intersect = intersect is null ? dates : new HashSet<DateOnly>(intersect.Intersect(dates));
            }
            if (intersect is null || intersect.Count < 2)
                throw new InvalidOperationException("Not enough overlapping dates across symbols.");

            var datesSorted = intersect.OrderBy(d => d).ToList();

            // 3) Map to aligned Close arrays per symbol
            var aligned = new Dictionary<string,double[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var (sym, lst) in px)
            {
                var map = lst.ToDictionary(t => t.Item1, t => t.Item2);
                var arr = datesSorted.Select(d => map[d]).ToArray();
                aligned[sym] = arr;
            }

            // 4) Build returns (simple) per symbol (length N-1)
            var rets = new Dictionary<string,double[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var (sym, arr) in aligned)
            {
                var r = new double[arr.Length - 1];
                for (int i = 1; i < arr.Length; i++)
                    r[i - 1] = (arr[i] - arr[i - 1]) / arr[i - 1];
                rets[sym] = r;
            }
            // dates for returns (skip first price date)
            var retDates = datesSorted.Skip(1).ToList();
            return (retDates, rets);
        }

        public static IEnumerable<(DateOnly date, string s1, string s2, double corr)>
            RollingPairwiseCorr(List<DateOnly> dates, Dictionary<string,double[]> rets, int window)
        {
            var syms = rets.Keys.OrderBy(s => s).ToArray();
            int n = dates.Count; // should match all rets lengths
            for (int i = window - 1; i < n; i++)
            {
                int start = i - (window - 1);
                foreach (var (i1, i2) in Pairs(syms.Length))
                {
                    var s1 = syms[i1];
                    var s2 = syms[i2];
                    double corr = Corr(rets[s1], rets[s2], start, window);
                    yield return (dates[i], s1, s2, corr);
                }
            }
        }

        public static (string[] syms, double[,] mat)
            LastWindowMatrix(Dictionary<string,double[]> rets, int totalLen, int window)
        {
            var syms = rets.Keys.OrderBy(s => s).ToArray();
            int start = totalLen - window;
            var mat = new double[syms.Length, syms.Length];
            for (int i = 0; i < syms.Length; i++)
            {
                for (int j = 0; j < syms.Length; j++)
                {
                    mat[i, j] = (i == j) ? 1.0 : Corr(rets[syms[i]], rets[syms[j]], start, window);
                }
            }
            return (syms, mat);
        }

        public static IEnumerable<(DateOnly date, string sym, double vol)>
            RollingVol(List<DateOnly> dates, Dictionary<string,double[]> rets, int window, double tradingDays = 252)
        {
            foreach (var (sym, r) in rets)
            {
                for (int i = window - 1; i < r.Length; i++)
                {
                    int start = i - (window - 1);
                    double mean = 0;
                    for (int k = 0; k < window; k++) mean += r[start + k];
                    mean /= window;

                    double var = 0;
                    for (int k = 0; k < window; k++)
                    {
                        var dx = r[start + k] - mean;
                        var += dx * dx;
                    }
                    var /= Math.Max(1, window - 1);
                    var sd = Math.Sqrt(var);
                    yield return (dates[i], sym, sd * Math.Sqrt(tradingDays));
                }
            }
        }

        private static IEnumerable<(int i, int j)> Pairs(int n)
        {
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    yield return (i, j);
        }

        private static double Corr(double[] a, double[] b, int start, int len)
        {
            double ma = 0, mb = 0;
            for (int k = 0; k < len; k++) { ma += a[start + k]; mb += b[start + k]; }
            ma /= len; mb /= len;

            double vA = 0, vB = 0, cov = 0;
            for (int k = 0; k < len; k++)
            {
                var da = a[start + k] - ma;
                var db = b[start + k] - mb;
                cov += da * db;
                vA += da * da;
                vB += db * db;
            }
            if (vA <= 0 || vB <= 0) return 0;
            return cov / Math.Sqrt(vA * vB);
        }
    }
}
