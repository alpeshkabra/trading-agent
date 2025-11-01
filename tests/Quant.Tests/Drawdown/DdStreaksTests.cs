using QuantFrameworks.Drawdown;
using Xunit;

namespace Quant.Tests.Drawdown
{
    public class DdStreaksTests
    {
        [Fact]
        public void Computes_Up_Down_Streaks()
        {
            var dir = Directory.CreateTempSubdirectory();
            string p = Path.Combine(dir.FullName, "p.csv");
            File.WriteAllText(p,
            @"Date,Open,High,Low,Close,Volume
            2024-01-01,0,0,0,100,0
            2024-01-02,0,0,0,110,0
            2024-01-03,0,0,0,105,0
            2024-01-04,0,0,0,107,0
            ");
            var rets = DdCalc.LoadReturns(p);
            var st = DdCalc.Streaks(rets);
            // Last return is + => upstreak should be >=1 and down=0
            var last = st.Last();
            Assert.True(last.up >= 1 && last.down == 0);
        }
    }
}
