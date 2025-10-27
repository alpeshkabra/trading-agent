namespace Backtesting.Core;

public readonly record struct Tick(DateTime Time, double Price, double Size);
