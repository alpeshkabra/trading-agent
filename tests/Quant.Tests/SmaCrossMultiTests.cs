using System;
using System.Collections.Generic;
using QuantFrameworks.Execution;
using QuantFrameworks.Feeds;
using QuantFrameworks.Strategy;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class SmaCrossMultiTests
    {
        [Fact]
        public void Routes_To_Per_Symbol_Strategy()
        {
            var multi = new SmaCrossMultiStrategy(new[]{"AAPL","MSFT"}, fast:1, slow:2);
            var bars = new[]{
                new Bar(new DateTime(2024,1,1),"AAPL",10,10,10,10,0),
                new Bar(new DateTime(2024,1,2),"AAPL",11,11,11,11,0), // AAPL buy
                new Bar(new DateTime(2024,1,1),"MSFT",20,20,20,20,0),
                new Bar(new DateTime(2024,1,2),"MSFT",19,19,19,19,0), // MSFT stays flat/down
            };
            var orders = new List<Order>();
            foreach (var b in bars) orders.AddRange(multi.OnBar(b));
            Assert.Contains(orders, o => o.Symbol=="AAPL" && o.Quantity>0);
            Assert.DoesNotContain(orders, o => o.Symbol=="MSFT" && o.Quantity>0);
        }
    }
}
