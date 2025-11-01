namespace QuantFrameworks.Signals
{
    public static class SignalRules
    {
        public static List<SignalRow> Generate(List<IndicatorRow> rows, SignalConfig cfg)
        {
            var list = new List<SignalRow>(rows.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var sig = TradeSignal.FLAT;
                string reason = "hold";

                // SMA cross
                if (cfg.SmaFast > 1 && cfg.SmaSlow > 1 && r.SmaFast is not null && r.SmaSlow is not null && i > 0 &&
                    rows[i - 1].SmaFast is not null && rows[i - 1].SmaSlow is not null)
                {
                    var prevAbove = rows[i - 1].SmaFast >= rows[i - 1].SmaSlow;
                    var nowAbove  = r.SmaFast >= r.SmaSlow;
                    if (!prevAbove && nowAbove) { sig = TradeSignal.BUY;  reason = "SMA cross up"; }
                    else if (prevAbove && !nowAbove) { sig = TradeSignal.SELL; reason = "SMA cross down"; }
                }

                // RSI rules (if no SMA signal)
                if (sig == TradeSignal.FLAT && cfg.Rsi > 0 && r.Rsi is not null)
                {
                    if (cfg.RsiBuy  is double rb && r.Rsi < rb)  { sig = TradeSignal.BUY;  reason = $"RSI<{rb}"; }
                    if (cfg.RsiSell is double rs && r.Rsi > rs)  { sig = TradeSignal.SELL; reason = $"RSI>{rs}"; }
                }

                list.Add(new SignalRow { Date = r.Date, Signal = sig, Reason = reason });
            }
            return list;
        }
    }
}
