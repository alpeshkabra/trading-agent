namespace Quant.Ledger;

public readonly record struct PositionSnapshot(DateOnly Date, string Symbol, double Quantity, double Price, double MarketValue);
