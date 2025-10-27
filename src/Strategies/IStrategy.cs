using Backtesting.Core;
using Backtesting.Models;

namespace Backtesting.Strategies;

public interface IStrategy
{
    void Initialize();
    IEnumerable<Order> OnData(Bar bar);
    void OnEnd();
}
