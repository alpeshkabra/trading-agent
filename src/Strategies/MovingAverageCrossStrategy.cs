using Backtesting.Core;
using Backtesting.Models;

namespace Backtesting.Strategies;

public class MovingAverageCrossStrategy : IStrategy
{
    private readonly int _fast;
    private readonly int _slow;
    private readonly Queue<double> _closes = new();
    private double? _prevFast;
    private double? _prevSlow;

    public MovingAverageCrossStrategy(int fast, int slow)
    {
        if (fast >= slow) throw new ArgumentException("fast must be < slow");
        _fast = fast; _slow = slow;
    }

    public void Initialize() { }

    public IEnumerable<Order> OnData(Bar bar)
    {
        _closes.Enqueue(bar.Close);
        while (_closes.Count > _slow) _closes.Dequeue();

        var closes = _closes.ToArray();
        if (closes.Length < _slow)
            yield break;

        var fast = closes.Skip(closes.Length - _fast).Average();
        var slow = closes.Average();

        if (_prevFast is not null && _prevSlow is not null)
        {
            if (_prevFast <= _prevSlow && fast > slow)
            {
                yield return new Order { Date = bar.Date, Side = OrderSide.Buy, Type = OrderType.Market, Quantity = 1, Tag = "SMA Cross Up" };
            }
            else if (_prevFast >= _prevSlow && fast < slow)
            {
                yield return new Order { Date = bar.Date, Side = OrderSide.Sell, Type = OrderType.Market, Quantity = 1, Tag = "SMA Cross Down" };
            }
        }

        _prevFast = fast;
        _prevSlow = slow;
    }

    public void OnEnd() { }
}
