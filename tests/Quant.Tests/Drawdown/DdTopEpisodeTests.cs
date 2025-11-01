using QuantFrameworks.Drawdown;
using Xunit;

namespace Quant.Tests.Drawdown
{
    public class DdTopEpisodeTests
    {
        [Fact]
        public void Detects_Trough_And_Recovery()
        {
            var dir = Directory.CreateTempSubdirectory();
            string p = Path.Combine(dir.FullName, "p.csv");
            // up → drop → recover above prior peak
            File.WriteAllText(p,
            @"Date,Open,High,Low,Close,Volume
            2024-01-01,0,0,0,100,0
            2024-01-02,0,0,0,110,0
            2024-01-03,0,0,0,90,0
            2024-01-04,0,0,0,111,0
            ");
            var rets = DdCalc.LoadReturns(p);
            var curve = DdCalc.BuildCurve("X", rets);
            var eps = DdCalc.TopDrawdowns("X", curve, 5);
            Assert.Contains(eps, e => e.RecoveryDate.HasValue && e.RecoveryDate.Value == new DateOnly(2024,1,4));
            Assert.Contains(eps, e => e.Depth < 0); // actual drawdown
        }
    }
}
