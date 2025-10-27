using Backtesting.Core;
using Backtesting.Models;

namespace Backtesting.Engine;

public class ExecutionSimulator
{
    public double Slippage { get; }

    public ExecutionSimulator(double slippage = 0.0) => Slippage = slippage;

    public Trade? Execute(Order order, Bar bar) => OrderMatcher.TryMatch(order, bar, Slippage);
}
