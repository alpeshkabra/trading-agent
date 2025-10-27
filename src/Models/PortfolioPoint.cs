namespace Quant.Models;

public readonly record struct PortfolioPoint(DateOnly Date, double Return, double Wealth);
