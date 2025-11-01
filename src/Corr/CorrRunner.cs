using System.Text;

namespace QuantFrameworks.Corr
{
    public static class CorrRunner
    {
        public static void Run(Dictionary<string,string> symbolPaths, CorrConfig cfg)
        {
            var (dates, rets) = CorrCalc.LoadAlignedReturns(symbolPaths);
            if (dates.Count < cfg.Window)
                throw new InvalidOperationException("Not enough observations for the requested window.");

            Directory.CreateDirectory(cfg.OutputDir);

            var rcPath = Path.Combine(cfg.OutputDir, "rolling_corr.csv");
            using (var sw = new StreamWriter(rcPath, false, Encoding.UTF8))
            {
                sw.WriteLine("Date,Sym1,Sym2,Corr");
                foreach (var row in CorrCalc.RollingPairwiseCorr(dates, rets, cfg.Window))
                    sw.WriteLine($"{row.date:yyyy-MM-dd},{row.s1},{row.s2},{row.corr.ToString("G17", System.Globalization.CultureInfo.InvariantCulture)}");
            }

            var (syms, mat) = CorrCalc.LastWindowMatrix(rets, dates.Count, cfg.Window);
            var lmPath = Path.Combine(cfg.OutputDir, "last_matrix.csv");
            using (var sw = new StreamWriter(lmPath, false, Encoding.UTF8))
            {
                sw.WriteLine("," + string.Join(",", syms));
                for (int i = 0; i < syms.Length; i++)
                {
                    var row = new string[syms.Length + 1];
                    row[0] = syms[i];
                    for (int j = 0; j < syms.Length; j++)
                        row[j + 1] = mat[i, j].ToString("G17", System.Globalization.CultureInfo.InvariantCulture);
                    sw.WriteLine(string.Join(",", row));
                }
            }

            var rvPath = Path.Combine(cfg.OutputDir, "rolling_vol.csv");
            using (var sw = new StreamWriter(rvPath, false, Encoding.UTF8))
            {
                sw.WriteLine("Date,Symbol,Vol");
                foreach (var row in CorrCalc.RollingVol(dates, rets, cfg.Window))
                    sw.WriteLine($"{row.date:yyyy-MM-dd},{row.sym},{row.vol.ToString("G17", System.Globalization.CultureInfo.InvariantCulture)}");
            }

            Console.WriteLine($"Wrote: {Path.GetFullPath(rcPath)}");
            Console.WriteLine($"Wrote: {Path.GetFullPath(lmPath)}");
            Console.WriteLine($"Wrote: {Path.GetFullPath(rvPath)}");
        }
    }
}
