using QuantFrameworks.IO;
using System.Globalization;
using System.Text;
using QuantFrameworks.Models;
using QuantFrameworks.Risk.Sizing;


namespace QuantFrameworks.Risk
{
    public static class RiskRunner
    {
        public static void Run(string ordersCsv, string configJson, string? pricesCsv, string outDir)
        {
            Directory.CreateDirectory(outDir);

            var cfg = RiskConfig.Load(configJson);
            var orders = CsvReaders.ReadOrders(ordersCsv);
            var px = pricesCsv is not null ? CsvReaders.ReadDailyCloses(pricesCsv) : new();

            var engine = new RiskEngine(cfg, px);

            var reportPath = Path.Combine(outDir, "risk_report.csv");
            var approvedPath = Path.Combine(outDir, "validated_orders.csv");

            using var rep = new StreamWriter(reportPath, false, Encoding.UTF8);
            rep.WriteLine("timestamp,symbol,side,input_qty,price,approved,final_qty,reasons");

            using var val = new StreamWriter(approvedPath, false, Encoding.UTF8);
            val.WriteLine("timestamp,symbol,side,qty,price");

            foreach (var o in orders)
            {
                var result = engine.Evaluate(o);

                rep.WriteLine(string.Join(",",
                    o.Timestamp.ToUniversalTime().ToString("o"),
                    o.Symbol,
                    o.Side,
                    o.Qty.ToString(CultureInfo.InvariantCulture),
                    o.Price.ToString(CultureInfo.InvariantCulture),
                    result.Approved ? "1" : "0",
                    result.FinalQty.ToString(CultureInfo.InvariantCulture),
                    string.Join("|", result.Reasons)));

                if (result.Approved && result.FinalQty != 0)
                {
                    val.WriteLine(string.Join(",",
                        o.Timestamp.ToUniversalTime().ToString("o"),
                        o.Symbol,
                        o.Side,
                        result.FinalQty.ToString(CultureInfo.InvariantCulture),
                        o.Price.ToString(CultureInfo.InvariantCulture)));
                }
            }

            Console.WriteLine($"Wrote: {reportPath}");
            Console.WriteLine($"Wrote: {approvedPath}");
        }
    }
}
