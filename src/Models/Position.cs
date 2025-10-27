namespace Backtesting.Models;

public class Position
{
    public double Quantity { get; private set; }
    public double AvgPrice { get; private set; }

    public void ApplyFill(OrderSide side, double qty, double price)
    {
        if (side == OrderSide.Buy)
        {
            var newQty = Quantity + qty;
            AvgPrice = (AvgPrice * Quantity + price * qty) / Math.Max(1e-9, newQty);
            Quantity = newQty;
        }
        else
        {
            Quantity -= qty;
            if (Quantity <= 1e-9) { Quantity = 0; AvgPrice = 0; }
        }
    }

    public double Unrealized(double lastPrice) => (lastPrice - AvgPrice) * Quantity;
}
