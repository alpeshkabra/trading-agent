using System.Text;
using System.Globalization;

namespace QuantFrameworks.Drawdown
{
    public static class DdRunner
    {
        public static void Run(Dictionary<string,string> symbols, DdConfig cfg)
        {
            Directory.CreateDirectory(cfg.OutputDir);

            var allCurves = new List<DdCalc.CurveRow>();
            var allEpisodes = new List<DdCalc.Episode>();
            var allStreaks = new List<(DateOnly date, string sym, int up, int down)>();

            foreach (var (sym, path) in symbols)
            {
                var rets = DdCalc.LoadReturns(path);
                var curve = DdCalc.BuildCurve(sym, rets);
                var top = DdCalc.TopDrawdowns(sym, curve, cfg.TopN);
                var streaks = DdCalc.Streaks(rets).Select(s => (s.date, sym, s.up, s.down)).ToList();

                allCurves.AddRange(curve);
                allEpisodes.AddRange(top);
                allStreaks.AddRange(streaks);
            }

            // dd_curve.csv
            var curvePath = Path.Combine(cfg.OutputDir, "dd_curve.csv");
            using (var sw = new StreamWriter(curvePath, false, Encoding.UTF8))
            {
                sw.WriteLine("Date,Symbol,NAV,Peak,Drawdown");
                foreach (var r in allCurves.OrderBy(r => r.Symbol).ThenBy(r => r.Date))
                    sw.WriteLine($"{r.Date:yyyy-MM-dd},{r.Symbol},{Fmt(r.Nav)},{Fmt(r.Peak)},{Fmt(r.Drawdown)}");
            }

            // top_drawdowns.csv
            var topPath = Path.Combine(cfg.OutputDir, "top_drawdowns.csv");
            using (var sw = new StreamWriter(topPath, false, Encoding.UTF8))
            {
                sw.WriteLine("Symbol,Rank,PeakDate,TroughDate,RecoveryDate,Depth,LengthDays,RecoveryDays");
                foreach (var grp in allEpisodes.GroupBy(e => e.Symbol))
                {
                    int rank = 1;
                    foreach (var e in grp.OrderBy(e => e.Depth))
                    {
                        sw.WriteLine($"{e.Symbol},{rank},{e.PeakDate:yyyy-MM-dd},{e.TroughDate:yyyy-MM-dd}," +
                                     $"{(e.RecoveryDate.HasValue ? e.RecoveryDate.Value.ToString("yyyy-MM-dd") : "")}," +
                                     $"{Fmt(e.Depth)},{e.LengthDays},{(e.RecoveryDays.HasValue ? e.RecoveryDays.Value.ToString(CultureInfo.InvariantCulture) : "")}");
                        rank++;
                    }
                }
            }

            // streaks.csv
            var stPath = Path.Combine(cfg.OutputDir, "streaks.csv");
            using (var sw = new StreamWriter(stPath, false, Encoding.UTF8))
            {
                sw.WriteLine("Date,Symbol,UpStreak,DownStreak");
                foreach (var s in allStreaks.OrderBy(s => s.sym).ThenBy(s => s.date))
                    sw.WriteLine($"{s.date:yyyy-MM-dd},{s.sym},{s.up},{s.down}");
            }

            Console.WriteLine($"Wrote: {Path.GetFullPath(curvePath)}");
            Console.WriteLine($"Wrote: {Path.GetFullPath(topPath)}");
            Console.WriteLine($"Wrote: {Path.GetFullPath(stPath)}");
        }

        private static string Fmt(double x) => x.ToString("G17", CultureInfo.InvariantCulture);
    }
}
