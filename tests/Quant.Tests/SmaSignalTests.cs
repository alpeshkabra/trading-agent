using System.Collections.Generic;
using QuantFrameworks.Feeds;
using QuantFrameworks.Strategy;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class SmaSignalTests
    {
        [Fact]
        public void SmaCross_Generates_Buy_Then_Sell()
        {
            var s = new SmaCrossStrategy("AAPL", fast:2, slow:3);
            var bars = new[]
            {
                new Bar(new System.DateTime(2024,1,1),"AAPL",1,1,1,1,0),
                new Bar(new System.DateTime(2024,1,2),"AAPL",1,1,1,2,0),
                new Bar(new System.DateTime(2024,1,3),"AAPL",1,1,1,3,0),
                new Bar(new System.DateTime(2024,1,4),"AAPL",1,1,1,1,0),
                new Bar(new System.DateTime(2024,1,5),"AAPL",1,1,1,0.5m,0),
            };

            var orders = new List<string>();
            foreach (var b in bars)
                foreach (var o in s.OnBar(b))
                    orders.Add($"{o.Tag}:{o.Quantity}");

            Assert.Contains("BUY:100", orders);
            Assert.Contains("SELL:-100", orders);
        }
    }
}