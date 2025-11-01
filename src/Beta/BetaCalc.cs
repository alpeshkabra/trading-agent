using Quant;

namespace QuantFrameworks.Beta
{
    public static class BetaCalc
    {
        // Load aligned returns of benchmark and symbols. Returns dates (N-1) + per-symbol arrays of length N-1
        public static (List<DateOnly> dates, double[] bench, Dictionary<string,double[]> asset)
            LoadAlignedReturns(string benchCsv, Dictionary<string,string> symPaths)
        {
            // read benchmark
            var bpx = new List<(DateOnly,double)>();
            foreach (var b in new CsvReader(benchCsv).ReadBars())
                if (b.Close > 0) bpx.Add((b.Date, b.Close));
            bpx.Sort((a,b) => a.Item1.CompareTo(b.Item1));

            // read assets
            var apx = new Dictionary<string, List<(DateOnly,double)>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in symPaths)
            {
                var lst = new List<(DateOnly,double)>();
                foreach (var r in new CsvReader(kv.Value).ReadBars())
                    if (r.Close > 0) lst.Add((r.Date, r.Close));
                lst.Sort((a,b) => a.Item1.CompareTo(b.Item1));
                apx[kv.Key] = lst;
            }

            // intersection of all dates
            HashSet<DateOnly>? inter = bpx.Select(t => t.Item1).ToHashSet();
            foreach (var lst in apx.Values)
                inter = new HashSet<DateOnly>(inter!.Intersect(lst.Select(t => t.Item1)));
            if (inter is null || inter.Count < 2)
                throw new InvalidOperationException("Not enough overlapping dates.");

            var dates = inter.OrderBy(d => d).ToList();

            // aligned prices
            var bmap = bpx.ToDictionary(t => t.Item1, t => t.Item2);
            var bArr = dates.Select(d => bmap[d]).ToArray();

            var assets = new Dictionary<string,double[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var (sym, lst) in apx)
            {
                var amap = lst.ToDictionary(t => t.Item1, t => t.Item2);
                assets[sym] = dates.Select(d => amap[d]).ToArray();
            }

            // build returns (length N-1)
            var benchR = ToReturns(bArr);
            var assetR = assets.ToDictionary(kv => kv.Key, kv => ToReturns(kv.Value),
                StringComparer.OrdinalIgnoreCase);

            // return dates correspond to prices[1..]
            var retDates = dates.Skip(1).ToList();
            return (retDates, benchR, assetR);
        }

        public static IEnumerable<(DateOnly date, string sym, double beta, double alpha, double r2, double corr)>
            Rolling(List<DateOnly> dates, double[] bench, Dictionary<string,double[]> asset, int window)
        {
            for (int i = window - 1; i < bench.Length; i++)
            {
                int start = i - (window - 1);
                // benchmark window stats
                var (mb, vb) = MeanVar(bench, start, window);

                foreach (var (sym, a) in asset)
                {
                    var (ma, _) = MeanVar(a, start, window);
                    var cov = Cov(a, bench, start, window, ma, mb);
                    var beta = vb <= 0 ? 0.0 : cov / vb;
                    var alpha = ma - beta * mb;
                    var corr = Corr(a, bench, start, window, ma, mb, vb);
                    var r2 = corr * corr;
                    yield return (dates[i], sym, beta, alpha, r2, corr);
                }
            }
        }

        public static IEnumerable<(string sym, int samples, double beta, double alpha, double r2, double corr)>
            Summary(double[] bench, Dictionary<string,double[]> asset)
        {
            var (mb, vb) = MeanVar(bench, 0, bench.Length);
            foreach (var (sym, a) in asset)
            {
                var (ma, va) = MeanVar(a, 0, a.Length);
                var cov = Cov(a, bench, 0, bench.Length, ma, mb);
                var beta = vb <= 0 ? 0.0 : cov / vb;
                var alpha = ma - beta * mb;
                var corr = Corr(a, bench, 0, bench.Length, ma, mb, vb);
                var r2 = corr * corr;
                yield return (sym, a.Length, beta, alpha, r2, corr);
            }
        }

        // helpers
        private static double[] ToReturns(double[] px)
        {
            var r = new double[px.Length - 1];
            for (int i = 1; i < px.Length; i++) r[i - 1] = (px[i] - px[i - 1]) / px[i - 1];
            return r;
        }
        private static (double mean, double var) MeanVar(double[] x, int start, int len)
        {
            double m = 0; for (int k = 0; k < len; k++) m += x[start + k]; m /= len;
            double v = 0; for (int k = 0; k < len; k++) { var d = x[start + k] - m; v += d * d; }
            v /= Math.Max(1, len - 1);
            return (m, v);
        }
        private static double Cov(double[] a, double[] b, int start, int len, double ma, double mb)
        {
            double c = 0; for (int k = 0; k < len; k++) c += (a[start + k] - ma) * (b[start + k] - mb);
            return c / Math.Max(1, len - 1);
        }
        private static double Corr(double[] a, double[] b, int start, int len, double ma, double mb, double vb)
        {
            double va = 0; for (int k = 0; k < len; k++) { var da = a[start + k] - ma; va += da * da; }
            if (va <= 0 || vb <= 0) return 0;
            var cov = Cov(a, b, start, len, ma, mb);
            return cov / Math.Sqrt(va * vb);
        }
    }
}
