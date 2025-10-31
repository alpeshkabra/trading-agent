using System.Collections.Generic;
using System.Linq;

namespace QuantFrameworks.Optimize
{
    public sealed class SweepResult
    {
        public List<RunResult> Runs { get; } = new();
        public string Metric { get; init; } = "Sharpe";

        public IEnumerable<RunResult> TopN(int n)
        {
            return Metric.ToLowerInvariant() switch
            {
                "nav"         => Runs.OrderByDescending(r => r.NAV).Take(n),
                "totalreturn" => Runs.OrderByDescending(r => r.TotalReturn).Take(n),
                _             => Runs.OrderByDescending(r => r.Sharpe).Take(n),
            };
        }
    }
}
