using Quant;

namespace QuantFrameworks.Drawdown
{
    public static class DdCalc
    {
        public sealed class CurveRow
        {
            public DateOnly Date;
            public string Symbol = "";
            public double Nav;      // cumulative wealth
            public double Peak;     // running max(Nav)
            public double Drawdown; // (Nav / Peak) - 1
        }

        public sealed class Episode
        {
            public string Symbol = "";
            public DateOnly PeakDate;
            public DateOnly TroughDate;
            public DateOnly? RecoveryDate; // null if not recovered by end
            public double Depth;           // negative number (e.g., -0.27)
            public int LengthDays;         // peak..trough inclusive
            public int? RecoveryDays;      // trough..recovery inclusive
        }

        public static List<(DateOnly date, double ret)> LoadReturns(string csvPath)
        {
            var bars = new CsvReader(csvPath).ReadBars().OrderBy(b => b.Date).ToList();
            var rets = new List<(DateOnly, double)>();
            for (int i = 1; i < bars.Count; i++)
            {
                var r = (bars[i].Close - bars[i-1].Close) / bars[i-1].Close;
                rets.Add((bars[i].Date, r));
            }
            return rets;
        }

        public static List<CurveRow> BuildCurve(string symbol, List<(DateOnly date, double ret)> rets)
        {
            var rows = new List<CurveRow>(rets.Count);
            double nav = 1.0, peak = 1.0;

            foreach (var (d, r) in rets)
            {
                nav *= (1.0 + r);
                if (nav > peak) peak = nav;
                var dd = (nav / peak) - 1.0;
                rows.Add(new CurveRow { Date = d, Symbol = symbol, Nav = nav, Peak = peak, Drawdown = dd });
            }
            return rows;
        }

        public static List<Episode> TopDrawdowns(string symbol, List<CurveRow> curve, int topN)
        {
            // Identify episodes by scanning for peak->trough->recovery
            var eps = new List<Episode>();
            int i = 0;
            while (i < curve.Count)
            {
                // find start at a peak (local where Nav == Peak)
                while (i < curve.Count && curve[i].Nav < curve[i].Peak) i++;
                if (i >= curve.Count) break;
                var peakIdx = i; var peakVal = curve[i].Peak; var peakDate = curve[i].Date;

                // walk forward until next new peak or end; track trough
                int troughIdx = i; double troughVal = curve[i].Nav; DateOnly troughDate = curve[i].Date;
                i++;
                while (i < curve.Count && curve[i].Peak <= peakVal)
                {
                    if (curve[i].Nav < troughVal) { troughVal = curve[i].Nav; troughDate = curve[i].Date; troughIdx = i; }
                    i++;
                }

                if (troughIdx > peakIdx) // we actually had a drawdown
                {
                    // try to find recovery date (first time Peak exceeds prior peak)
                    DateOnly? recDate = null;
                    if (i < curve.Count && curve[i].Peak > peakVal)
                        recDate = curve[i].Date;
                    // depth and lengths
                    var depth = (troughVal / peakVal) - 1.0; // negative
                    var lenDays = (troughDate.ToDateTime(TimeOnly.MinValue) - peakDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
                    int? recDays = recDate.HasValue
                        ? (recDate.Value.ToDateTime(TimeOnly.MinValue) - troughDate.ToDateTime(TimeOnly.MinValue)).Days + 1
                        : (int?)null;
                    eps.Add(new Episode {
                        Symbol = symbol, PeakDate = peakDate, TroughDate = troughDate, RecoveryDate = recDate,
                        Depth = depth, LengthDays = lenDays, RecoveryDays = recDays
                    });
                }
            }

            // rank by Depth asc (more negative is worse)
            return eps.OrderBy(e => e.Depth).Take(topN).ToList();
        }

        public static List<(DateOnly date, int up, int down)> Streaks(List<(DateOnly date, double ret)> rets)
        {
            var outRows = new List<(DateOnly, int, int)>(rets.Count);
            int up = 0, down = 0;
            foreach (var (d, r) in rets)
            {
                if (r > 0) { up += 1; down = 0; }
                else if (r < 0) { down += 1; up = 0; }
                else { /* flat */ }
                outRows.Add((d, up, down));
            }
            return outRows;
        }
    }
}
