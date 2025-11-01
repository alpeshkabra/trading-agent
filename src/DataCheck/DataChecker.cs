using System.Globalization;
using Quant;

namespace QuantFrameworks.DataCheck
{
    public static class DataChecker
    {
        public static (DqxSummary summary, List<Anomaly> anomalies) CheckCsv(string path, DqxConfig cfg)
        {
            var rows = new List<(DateOnly d, double o, double h, double l, double c, long v)>();
            foreach (var b in new CsvReader(path).ReadBars())
                rows.Add((b.Date, b.Open, b.High, b.Low, b.Close, (long)b.Volume));

            var anomalies = new List<Anomaly>();
            var sum = new DqxSummary { Rows = rows.Count };

            if (rows.Count == 0) return (sum, anomalies);

            // duplicates / unsorted
            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i].d == rows[i - 1].d)
                {
                    anomalies.Add(new Anomaly { Date = rows[i].d, Kind = AnomalyKind.Duplicate, Detail = "duplicate date" });
                    sum.Duplicates++;
                }
                if (rows[i].d < rows[i - 1].d)
                {
                    anomalies.Add(new Anomaly { Date = rows[i].d, Kind = AnomalyKind.Unsorted, Detail = "date order descending" });
                    sum.Unsorted++;
                }
            }

            // gaps
            for (int i = 1; i < rows.Count; i++)
            {
                var gap = rows[i].d.DayNumber - rows[i - 1].d.DayNumber;
                if (gap > cfg.MaxGapDays)
                {
                    anomalies.Add(new Anomaly { Date = rows[i].d, Kind = AnomalyKind.Gap, Detail = $"gap {gap} days" });
                    sum.Gaps++;
                }
            }

            // zero/negative + outliers
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                if (r.o <= 0 || r.h <= 0 || r.l <= 0 || r.c <= 0 || r.v < cfg.MinVolume)
                {
                    anomalies.Add(new Anomaly { Date = r.d, Kind = AnomalyKind.ZeroOrNegative, Detail = "zero/neg price or low volume" });
                    sum.ZeroOrNegative++;
                }

                if (i > 0)
                {
                    var prev = rows[i - 1];
                    if (prev.c > 0)
                    {
                        var ret = Math.Abs((r.c - prev.c) / prev.c);
                        if (ret > cfg.MaxAbsReturn)
                        {
                            anomalies.Add(new Anomaly { Date = r.d, Kind = AnomalyKind.Outlier, Detail = $"absRet={ret.ToString("P2", CultureInfo.InvariantCulture)}" });
                            sum.Outliers++;
                        }
                    }
                }
            }

            return (sum, anomalies);
        }
    }
}
