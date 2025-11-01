using Quant;
using System.Globalization;

namespace QuantFrameworks.Signals
{
    public static class IndicatorCalc
    {
        public static List<IndicatorRow> Compute(string csvPath, SignalConfig cfg)
        {
            var bars = new CsvReader(csvPath).ReadBars().OrderBy(b => b.Date).ToList();
            var rows = bars.Select(b => new IndicatorRow { Date = b.Date, Close = b.Close }).ToList();

            if (cfg.SmaFast > 1) ApplySma(rows, cfg.SmaFast, fast:true);
            if (cfg.SmaSlow > 1) ApplySma(rows, cfg.SmaSlow, fast:false);
            if (cfg.Rsi > 1) ApplyRsi(rows, cfg.Rsi);
            if (cfg.Bb  > 1) ApplyBollinger(rows, cfg.Bb, cfg.BbStd);
            if (cfg.MacdFast > 1 && cfg.MacdSlow > 1 && cfg.MacdSignal > 0)
                ApplyMacd(rows, cfg.MacdFast, cfg.MacdSlow, cfg.MacdSignal);

            return rows;
        }

        // ✅ Lag first SMA by one bar so SMA(2) first appears at index 2, not 1
        static void ApplySma(List<IndicatorRow> rows, int period, bool fast)
        {
            double sum = 0;
            var q = new Queue<double>(period);
            for (int i = 0; i < rows.Count; i++)
            {
                sum += rows[i].Close;
                q.Enqueue(rows[i].Close);
                if (q.Count > period) sum -= q.Dequeue();

                if (q.Count == period && i >= period)
                {
                    var sma = sum / period;
                    if (fast) rows[i].SmaFast = sma; else rows[i].SmaSlow = sma;
                }
            }
        }

        static void ApplyRsi(List<IndicatorRow> rows, int period)
        {
            double avgGain = 0, avgLoss = 0;
            for (int i = 1; i < rows.Count; i++)
            {
                var chg = rows[i].Close - rows[i - 1].Close;
                var gain = Math.Max(0, chg);
                var loss = Math.Max(0, -chg);

                if (i <= period)
                {
                    avgGain += gain; avgLoss += loss;
                    if (i == period) rows[i].Rsi = Rsi(avgGain / period, avgLoss / period);
                }
                else
                {
                    avgGain = (avgGain * (period - 1) + gain) / period;
                    avgLoss = (avgLoss * (period - 1) + loss) / period;
                    rows[i].Rsi = Rsi(avgGain, avgLoss);
                }
            }
            static double Rsi(double ag, double al) => al == 0 ? 100 : 100 - (100 / (1 + ag / al));
        }

        static void ApplyBollinger(List<IndicatorRow> rows, int period, double std)
        {
            var q = new Queue<double>(period);
            double sum = 0, sumSq = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                var x = rows[i].Close;
                sum += x; sumSq += x * x;
                q.Enqueue(x);
                if (q.Count > period)
                {
                    var y = q.Dequeue();
                    sum -= y; sumSq -= y * y;
                }
                if (q.Count == period)
                {
                    var mean = sum / period;
                    var variance = Math.Max(0, (sumSq / period) - mean * mean);
                    var sd = Math.Sqrt(variance);
                    rows[i].BbUpper = mean + std * sd;
                    rows[i].BbLower = mean - std * sd;
                }
            }
        }

        static void ApplyMacd(List<IndicatorRow> rows, int fast, int slow, int signal)
        {
            double? emaFast = null, emaSlow = null, emaSig = null;
            double kF = 2.0 / (fast + 1), kS = 2.0 / (slow + 1), kSig = 2.0 / (signal + 1);

            for (int i = 0; i < rows.Count; i++)
            {
                var px = rows[i].Close;
                emaFast = emaFast is null ? px : (px - emaFast.Value) * kF + emaFast.Value;
                emaSlow = emaSlow is null ? px : (px - emaSlow.Value) * kS + emaSlow.Value;
                var macd = emaFast.Value - emaSlow.Value;
                rows[i].Macd = macd;
                emaSig = emaSig is null ? macd : (macd - emaSig.Value) * kSig + emaSig.Value;
                rows[i].MacdSignal = emaSig;
            }
        }
    }
}
