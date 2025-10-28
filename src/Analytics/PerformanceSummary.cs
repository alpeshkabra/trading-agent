namespace Quant.Models;

public class PerformanceSummary
{
    public string Label { get; init; } = "";
    public int Observations { get; init; }
    public double TotalReturn { get; init; }
    public double AnnualizedVol { get; init; }
    public double Sharpe { get; init; }
    public double MaxDrawdown { get; init; }
}
