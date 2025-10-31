using System.Globalization;
using System.IO;
using System.Linq;

namespace QuantFrameworks.Optimize
{
    public static class OptimizeCsvWriter
    {
        public static void WriteSweep(SweepResult r, string outDir)
        {
            Directory.CreateDirectory(outDir);
            var path = Path.Combine(outDir, "sweep_results.csv");
            using var sw = new StreamWriter(path);
            sw.WriteLine("Params,NAV,Sharpe,TotalReturn");
            foreach (var rr in r.Runs)
                sw.WriteLine($"{rr.Label},{rr.NAV.ToString(CultureInfo.InvariantCulture)},{rr.Sharpe.ToString(CultureInfo.InvariantCulture)},{rr.TotalReturn.ToString(CultureInfo.InvariantCulture)}");
        }

        public static void WriteTopN(SweepResult r, string outDir, int n)
        {
            Directory.CreateDirectory(outDir);
            var path = Path.Combine(outDir, "topN.csv");
            using var sw = new StreamWriter(path);
            sw.WriteLine("Rank,Params,Metric,Value");
            int rank = 1;
            foreach (var rr in r.TopN(n))
            {
                var (name, val) = r.Metric.ToLowerInvariant() switch
                {
                    "nav"         => ("NAV", rr.NAV),
                    "totalreturn" => ("TotalReturn", rr.TotalReturn),
                    _             => ("Sharpe", rr.Sharpe)
                };
                sw.WriteLine($"{rank},{rr.Label},{name},{val.ToString(CultureInfo.InvariantCulture)}");
                rank++;
            }
        }

        public static void WriteWfo(WfoResult w, string outDir)
        {
            Directory.CreateDirectory(outDir);
            var path = Path.Combine(outDir, "wfo_results.csv");
            using var sw = new StreamWriter(path);
            sw.WriteLine("Fold,TrainStart,TrainEnd,TestStart,TestEnd,BestParams,TestNAV,TestSharpe");
            for (int i = 0; i < w.Folds.Count; i++)
            {
                var f = w.Folds[i];
                sw.WriteLine($"{i+1},{f.TrainStart:yyyy-MM-dd},{f.TrainEnd:yyyy-MM-dd},{f.TestStart:yyyy-MM-dd},{f.TestEnd:yyyy-MM-dd},{f.BestParams},{f.TestNAV.ToString(CultureInfo.InvariantCulture)},{f.TestSharpe.ToString(CultureInfo.InvariantCulture)}");
            }
        }
    }
}
