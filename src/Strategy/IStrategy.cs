using System.Collections.Generic;
using QuantFrameworks.Feeds;
using QuantFrameworks.Execution;

namespace QuantFrameworks.Strategy
{
    public interface IStrategy
    {
        IEnumerable<Order> OnBar(Bar bar);
    }
}