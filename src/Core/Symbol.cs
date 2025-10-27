namespace Backtesting.Core;

public readonly record struct Symbol(string Ticker)
{
    public override string ToString() => Ticker;
}
