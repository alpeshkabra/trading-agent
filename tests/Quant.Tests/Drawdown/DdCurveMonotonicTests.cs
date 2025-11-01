using QuantFrameworks.Drawdown;
using Xunit;

namespace Quant.Tests.Drawdown
{
    public class DdCurveMonotonicTests
    {
        [Fact]
        public void Monotonic_Up_Has_Zero_MaxDD()
        {
            var dir = Directory.CreateTempSubdirectory();
            string p = Path.Combine(dir.FullName, "p.csv");
            File.WriteAllText(p,
            @"Date,Open,High,Low,Close,Volume
            2024-01-01,0,0,0,100,0
            2024-01-02,0,0,0,101,0
            2024-01-03,0,0,0,102,0
            2024-01-04,0,0,0,103,0
            ");
            var rets = DdCalc.LoadReturns(p);
            var curve = DdCalc.BuildCurve("X", rets);
            var maxDd = curve.Min(r => r.Drawdown);
            Assert.True(maxDd >= -1e-12); // essentially 0
        }
    }
}
