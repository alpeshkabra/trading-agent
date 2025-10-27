namespace Backtesting.Core;

public readonly record struct Bar(DateOnly Date, double Open, double High, double Low, double Close, double Volume)
{
    public double Mid => (High + Low) * 0.5;
    public double Range => High - Low;
    public override string ToString() => $"{Date:yyyy-MM-dd} O:{Open:F2} H:{High:F2} L:{Low:F2} C:{Close:F2} V:{Volume:F0}";
}
