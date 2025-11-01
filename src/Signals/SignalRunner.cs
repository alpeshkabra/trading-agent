using System.Text;

namespace QuantFrameworks.Signals
{
    public static class SignalRunner
    {
        public static void Run(string csvPath, string outDir, SignalConfig cfg)
        {
            Directory.CreateDirectory(outDir);

            var rows = IndicatorCalc.Compute(csvPath, cfg);
            var sigs = SignalRules.Generate(rows, cfg);

            var indPath = Path.Combine(outDir, "indicators.csv");
            using (var sw = new StreamWriter(indPath, false, Encoding.UTF8))
            {
                sw.WriteLine("Date,Close,SMA_fast,SMA_slow,RSI,BB_upper,BB_lower,MACD,MACD_signal");
                foreach (var r in rows)
                {
                    sw.WriteLine($"{r.Date:yyyy-MM-dd},{r.Close}," +
                                 $"{Fmt(r.SmaFast)},{Fmt(r.SmaSlow)},{Fmt(r.Rsi)}," +
                                 $"{Fmt(r.BbUpper)},{Fmt(r.BbLower)}," +
                                 $"{Fmt(r.Macd)},{Fmt(r.MacdSignal)}");
                }
            }

            var sigPath = Path.Combine(outDir, "signals.csv");
            using (var sw = new StreamWriter(sigPath, false, Encoding.UTF8))
            {
                sw.WriteLine("Date,Signal,Reason");
                foreach (var s in sigs)
                    sw.WriteLine($"{s.Date:yyyy-MM-dd},{s.Signal},{s.Reason.Replace(',', ';')}");
            }
        }

        static string Fmt(double? x) => x is null ? "" : x.Value.ToString("G17", System.Globalization.CultureInfo.InvariantCulture);
    }
}
