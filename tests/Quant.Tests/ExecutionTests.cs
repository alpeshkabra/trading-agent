using System;
using System.Collections.Generic;
using QuantFrameworks.Execution;
using QuantFrameworks.Feeds;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class ExecutionTests
    {
        [Fact]
        public void Market_Order_Fills_At_Next_Open()
        {
            var broker = new SimpleBrokerSimulator();
            var orders = new[] { new Order("AAPL", 100, OrderType.Market, Tag:"BUY") };
            var nextBar = new Bar(new DateTime(2024,1,2), "AAPL", 10, 12, 9, 11, 1000);

            var fills = new List<Fill>(broker.Match(orders, nextBar, nextBar.Date));
            Assert.Single(fills);
            Assert.Equal(10m, fills[0].Price);
        }

        [Fact]
        public void Limit_Buy_Fills_If_Low_Crosses()
        {
            var broker = new SimpleBrokerSimulator();
            var orders = new[] { new Order("AAPL", 100, OrderType.Limit, 9.5m, "BUY") };
            var nextBar = new Bar(new DateTime(2024,1,2), "AAPL", 10, 12, 9, 11, 1000);

            var fills = new List<Fill>(broker.Match(orders, nextBar, nextBar.Date));
            Assert.Single(fills);
            Assert.True(fills[0].Price <= 10m);
        }
    }
}