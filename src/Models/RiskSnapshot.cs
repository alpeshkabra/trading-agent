namespace Quant.Models;

public readonly record struct RiskSnapshot(
    DateOnly Date,
    double Volatility,
    double DownsideDev,
    double VaR,
    double CVar
);
