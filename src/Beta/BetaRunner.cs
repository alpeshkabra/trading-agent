using System.Text;

namespace QuantFrameworks.Beta
{
    public static class BetaRunner
    {
        public static void Run(Dictionary<string,string> symbols, string benchCsv, BetaConfig cfg)
        {
            var (dates, bench, asset) = BetaCalc.LoadAlignedReturns(benchCsv, symbols);
            if (bench.Length < cfg.Window)
                throw new InvalidOperationException("Not enough observations for requested window.");

            Directory.CreateDirectory(cfg.OutputDir);

            var rollPath = Path.Combine(cfg.OutputDir, "rolling_beta.csv");
            using (var sw = new StreamWriter(rollPath, false, Encoding.UTF8))
            {
                sw.WriteLine("Date,Symbol,Beta,Alpha,R2,Corr");
                foreach (var row in BetaCalc.Rolling(dates, bench, asset, cfg.Window))
                {
                    sw.WriteLine($"{row.date:yyyy-MM-dd},{row.sym}," +
                                 $"{Fmt(row.beta)},{Fmt(row.alpha)},{Fmt(row.r2)},{Fmt(row.corr)}");
                }
            }

            var sumPath = Path.Combine(cfg.OutputDir, "summary_beta.csv");
            using (var sw = new StreamWriter(sumPath, false, Encoding.UTF8))
            {
                sw.WriteLine("Symbol,Samples,Beta,Alpha,R2,Corr");
                foreach (var row in BetaCalc.Summary(bench, asset))
                    sw.WriteLine($"{row.sym},{row.samples},{Fmt(row.beta)},{Fmt(row.alpha)},{Fmt(row.r2)},{Fmt(row.corr)}");
            }

            Console.WriteLine($"Wrote: {Path.GetFullPath(rollPath)}");
            Console.WriteLine($"Wrote: {Path.GetFullPath(sumPath)}");
        }

        private static string Fmt(double x) => x.ToString("G17", System.Globalization.CultureInfo.InvariantCulture);
    }
}
