using Quant.Models;

namespace Quant.Reports
{
    public static class PortfolioCsv
    {
        public static void Write(string path, IEnumerable<PortfolioPoint> points)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            using var sw = new StreamWriter(path);
            sw.WriteLine("Date,Return,Wealth");
            foreach (var p in points)
            {
                sw.WriteLine($"{p.Date:yyyy-MM-dd},{p.Return:F6},{p.Wealth:F6}");
            }
        }
    }
}
