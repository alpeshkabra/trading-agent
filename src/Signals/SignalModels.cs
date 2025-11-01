namespace QuantFrameworks.Signals
{
    public sealed class SignalConfig
    {
        public int SmaFast { get; set; }
        public int SmaSlow { get; set; }
        public int Rsi { get; set; }
        public double? RsiBuy { get; set; }
        public double? RsiSell { get; set; }
        public int Bb { get; set; }
        public double BbStd { get; set; } = 2.0;
        public int MacdFast { get; set; }
        public int MacdSlow { get; set; }
        public int MacdSignal { get; set; }
    }
    public enum TradeSignal { FLAT, BUY, SELL }
    public sealed class IndicatorRow { public DateOnly Date; public double Close; public double? SmaFast; public double? SmaSlow; public double? Rsi; public double? BbUpper; public double? BbLower; public double? Macd; public double? MacdSignal; }
    public sealed class SignalRow { public DateOnly Date; public TradeSignal Signal; public string Reason = ""; }
}
