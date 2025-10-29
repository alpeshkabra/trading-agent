namespace Quant.Ledger;

public readonly record struct Trade(DateOnly Date, string Symbol, double Quantity, double Price, double Fees = 0.0);
