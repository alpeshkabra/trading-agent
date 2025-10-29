using System;
using System.Collections.Generic;
using QuantFrameworks.Feeds;

namespace QuantFrameworks.Execution
{
    /// <summary>Very small broker: Market fills at next bar Open; Limit fills if bar range crosses limit (price improved to min(limit, open) for buys, max(limit, open) for sells).</summary>
    public sealed class SimpleBrokerSimulator
    {
        public IEnumerable<Fill> Match(IEnumerable<Order> orders, Bar nextBar, DateTime time)
        {
            foreach (var o in orders)
            {
                if (!string.Equals(o.Symbol, nextBar.Symbol, StringComparison.OrdinalIgnoreCase)) continue;

                if (o.Type == OrderType.Market)
                {
                    yield return new Fill(o.Symbol, o.Quantity, nextBar.Open, time, o.Tag);
                }
                else if (o.Type == OrderType.Limit && o.LimitPrice.HasValue)
                {
                    var lim = o.LimitPrice.Value;
                    var crossed = o.Quantity > 0
                        ? nextBar.Low <= lim
                        : nextBar.High >= lim;

                    if (crossed)
                    {
                        decimal px;
                        if (o.Quantity > 0) px = Math.Min(lim, nextBar.Open);
                        else px = Math.Max(lim, nextBar.Open);

                        yield return new Fill(o.Symbol, o.Quantity, px, time, o.Tag);
                    }
                }
            }
        }
    }
}