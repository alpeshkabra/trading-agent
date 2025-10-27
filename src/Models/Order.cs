namespace Backtesting.Models;

public class Order
{
    public Guid Id { get; } = Guid.NewGuid();
    public OrderSide Side { get; init; }
    public OrderType Type { get; init; }
    public double Quantity { get; init; }
    public double? LimitPrice { get; init; }
    public DateOnly Date { get; init; }
    public string Tag { get; init; } = "";

    public override string ToString() => $"{Date:yyyy-MM-dd} {Side} {Quantity} @ {Type}" + (LimitPrice is null ? "" : $" Lmt {LimitPrice:F2}") + (string.IsNullOrWhiteSpace(Tag) ? "" : $" [{Tag}]");
}
