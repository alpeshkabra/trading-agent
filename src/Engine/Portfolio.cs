using Backtesting.Core;
using Backtesting.Models;
using Backtesting.Utils;

namespace Backtesting.Engine;

public class EquityPoint
{
    public DateOnly Date { get; init; }
    public double Equity { get; init; }
}

public class Portfolio
{
    public double Cash { get; private set; }
    public Position Position { get; } = new();
    public List<EquityPoint> EquityCurve { get; } = new();

    public Portfolio(double startingCash) => Cash = startingCash;

    public void ApplyFill(DateOnly date, OrderSide side, double qty, double price)
    {
        var cost = qty * price;
        if (side == OrderSide.Buy) Cash -= cost;
        else Cash += cost;
        Position.ApplyFill(side, qty, price);
        Logger.Debug($"Fill: {date:yyyy-MM-dd} {side} {qty} @ {price:F2} | Cash={Cash:F2} PosQty={Position.Quantity} Avg={Position.AvgPrice:F2}");
    }

    public void MarkToMarket(DateOnly date, double price)
    {
        var equity = Cash + Position.Quantity * price;
        EquityCurve.Add(new EquityPoint { Date = date, Equity = equity });
    }
}
