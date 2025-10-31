using QuantFrameworks.Reporting.Charts;
using Xunit;

namespace Quant.Tests.Reporting
{
    public class SimpleSvgChartTests
    {
        [Fact]
        public void Sparkline_Renders_SVG()
        {
            var svg = SimpleSvgChart.Sparkline(new decimal[]{1,2,3,2,1});
            Assert.StartsWith("<svg", svg.Trim());
            Assert.Contains("polyline", svg);
        }

        [Fact]
        public void FilledDrawdown_Renders_SVG()
        {
            var svg = SimpleSvgChart.FilledDrawdown(new decimal[]{100,105,102,110,90,92});
            Assert.StartsWith("<svg", svg.Trim());
            Assert.Contains("path", svg);
        }
    }
}
