namespace QuantFrameworks.Models
{
    public readonly record struct Order(DateTime Timestamp, string Symbol, string Side, int Qty, decimal Price);
}
