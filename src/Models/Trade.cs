namespace Backtesting.Models;

public class Trade
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateOnly Date { get; init; }
    public OrderSide Side { get; init; }
    public double Quantity { get; init; }
    public double Price { get; init; }
    public string Tag { get; init; } = "";

    public override string ToString() => $"{Date:yyyy-MM-dd} {Side} {Quantity} @ {Price:F2} {Tag}";
}
