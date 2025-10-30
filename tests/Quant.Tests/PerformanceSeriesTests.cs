using System.Collections.Generic;
using QuantFrameworks.Reporting;
using Xunit;

namespace Quant.Tests.Backtest
{
    public class PerformanceSeriesTests
    {
        [Fact]
        public void Wealth_And_MaxDD()
        {
            var wealth = PerformanceSeries.WealthFromNAV(new List<decimal>{100m, 110m, 105m});
            Assert.Equal(1.0m, wealth[0]);
            var (dd, _, _) = PerformanceSeries.MaxDrawdown(new List<decimal>{1.0m,1.2m,1.1m,1.3m,1.05m});
            Assert.True(dd > 0m);
        }
    }
}
