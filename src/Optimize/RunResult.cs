namespace QuantFrameworks.Optimize
{
    public sealed class RunResult
    {
        public ParamSet Params { get; init; } = new();
        public decimal NAV { get; init; }
        public decimal Sharpe { get; init; }
        public decimal TotalReturn { get; init; }
        public string Label { get; init; } = "";
    }
}
