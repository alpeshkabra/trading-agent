using System;
using System.Collections.Generic;
using QuantFrameworks.Execution;
using QuantFrameworks.Feeds;
using QuantFrameworks.Strategy;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class StopLossTakeProfitTests
    {
        [Fact]
        public void StopLoss_Triggers_Exit_When_Low_Crosses()
        {
            // Use fast:1 slow:2 so we get a BUY on bar 2.
            // Bar1 close=10; Bar2 close=12 -> fast=12, slow=avg(10,12)=11 => fast>slow => BUY.
            // Entry ~ 12, stopLossPct=10% => stop level = 10.8. Bar3 low=10 crosses -> STOP_EXIT.
            var s = new SmaCrossStrategy("AAPL", fast:1, slow:2, stopLossPct:0.10m, takeProfitPct:0m);

            var bars = new[]
            {
                new Bar(new DateTime(2024,1,1),"AAPL",10,10,10,10,0), // warm up
                new Bar(new DateTime(2024,1,2),"AAPL",12,12,12,12,0), // BUY (fast>slow)
                new Bar(new DateTime(2024,1,3),"AAPL",11,11,10,11,0)  // low=10 <= 10.8 => STOP_EXIT
            };

            var orders = new List<Order>();
            foreach (var b in bars)
                orders.AddRange(s.OnBar(b));

            Assert.Contains(orders, o => o.Tag == "BUY" && o.Quantity > 0);
            Assert.Contains(orders, o => o.Tag == "STOP_EXIT" && o.Quantity < 0);
        }

        [Fact]
        public void TakeProfit_Triggers_Exit_When_High_Crosses()
        {
            // fast:1 slow:2 to get a BUY on bar 2.
            // Bar1 close=10; Bar2 close=11 -> fast=11, slow=10.5 => BUY at ~11.
            // TP 10% => 12.1. Bar3 high=12.5 crosses -> TP_EXIT.
            var s = new SmaCrossStrategy("AAPL", fast:1, slow:2, stopLossPct:0m, takeProfitPct:0.10m);

            var bars = new[]
            {
                new Bar(new DateTime(2024,1,1),"AAPL",10,10,10,10,0),
                new Bar(new DateTime(2024,1,2),"AAPL",11,11,11,11,0), // BUY
                new Bar(new DateTime(2024,1,3),"AAPL",12,12.5m,11.5m,12,0) // high=12.5 >= 12.1 => TP_EXIT
            };

            var orders = new List<Order>();
            foreach (var b in bars)
                orders.AddRange(s.OnBar(b));

            Assert.Contains(orders, o => o.Tag == "BUY" && o.Quantity > 0);
            Assert.Contains(orders, o => o.Tag == "TP_EXIT" && o.Quantity < 0);
        }
    }
}
