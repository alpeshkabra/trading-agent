using QuantFrameworks.Models;
using QuantFrameworks.Risk.Sizing;

namespace QuantFrameworks.Risk
{
    public sealed class RiskEngine
    {
        private readonly RiskConfig _cfg;
        private readonly Dictionary<(DateOnly date, string symbol), decimal> _px;
        private readonly IPositionSizer _sizer;
        private decimal _aggregateExposureToday = 0m;

        public RiskEngine(RiskConfig cfg, Dictionary<(DateOnly,string),decimal> prices)
        {
            _cfg = cfg;
            _px = prices;
            _sizer = PositionSizerFactory.Create(cfg, prices);
        }

        public RiskResult Evaluate(Order o)
        {
            var reasons = new List<string>();
            var approved = true;

            if (_cfg.Blacklist.Contains(o.Symbol))
            {
                approved = false; reasons.Add("blacklisted");
            }

            // compute exposure if we executed as-is (or after sizing)
            var desiredQty = _sizer.Size(o); // may ignore o.Qty depending on mode
            if (_cfg.PerSymbolMaxQty.TryGetValue(o.Symbol, out var maxQty) && Math.Abs(desiredQty) > Math.Abs(maxQty))
            {
                desiredQty = Math.Sign(desiredQty) * maxQty;
                reasons.Add("clamped:perSymbolMaxQty");
            }

            var notional = Math.Abs(desiredQty) * o.Price;

            if (notional > _cfg.MaxPerSymbolExposure) { approved = false; reasons.Add("exceeds:maxPerSymbolExposure"); }

            if (_aggregateExposureToday + notional > _cfg.MaxDailyNotional)
            {
                approved = false; reasons.Add("exceeds:maxDailyNotional");
            }

            // simplistic “aggregate exposure” tracker
            if (notional > _cfg.MaxAggregateExposure) { approved = false; reasons.Add("exceeds:maxAggregateExposure"); }

            if (approved) _aggregateExposureToday += notional;

            return new RiskResult(approved, desiredQty, reasons);
        }
    }

    public readonly record struct RiskResult(bool Approved, int FinalQty, IReadOnlyList<string> Reasons);
}
