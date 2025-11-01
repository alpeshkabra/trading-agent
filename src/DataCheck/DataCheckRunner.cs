using System.Globalization;
using System.Text;

namespace QuantFrameworks.DataCheck
{
    public static class DataCheckRunner
    {
        public static DqxSummary Run(string csvPath, string outDir, DqxConfig cfg)
        {
            var (summary, anomalies) = DataChecker.CheckCsv(csvPath, cfg);

            var rep = Path.Combine(outDir, "report.csv");
            using (var sw = new StreamWriter(rep, false, Encoding.UTF8))
            {
                sw.WriteLine("rows,gaps,duplicates,unsorted,outliers,badRows,zeroOrNegative,totalIssues");
                sw.WriteLine($"{summary.Rows},{summary.Gaps},{summary.Duplicates},{summary.Unsorted},{summary.Outliers},{summary.BadRows},{summary.ZeroOrNegative},{summary.TotalIssues}");
            }

            var det = Path.Combine(outDir, "anomalies.csv");
            using (var sw = new StreamWriter(det, false, Encoding.UTF8))
            {
                sw.WriteLine("date,kind,detail");
                foreach (var a in anomalies)
                    sw.WriteLine($"{a.Date:yyyy-MM-dd},{a.Kind},{a.Detail.Replace(',', ';')}");
            }

            Console.WriteLine($"Wrote: {Path.GetFullPath(rep)}");
            Console.WriteLine($"Wrote: {Path.GetFullPath(det)}");
            return summary;
        }
    }
}
