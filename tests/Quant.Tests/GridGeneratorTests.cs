using System.Linq;
using QuantFrameworks.Optimize;
using Xunit;

namespace Quant.Tests.Optimize
{
    public class GridGeneratorTests
    {
        [Fact]
        public void Cartesian_Expands()
        {
            var specs = new[]
            {
                new ParamSpec { Name="Fast", From=2, To=4, Step=2 },  // 2,4
                new ParamSpec { Name="Slow", Values = new(){ 10, 20 } }
            };
            var grid = GridGenerator.Cartesian(specs).ToList();
            Assert.Equal(4, grid.Count);
            Assert.Contains(grid, p => p.Values["Fast"]==2 && p.Values["Slow"]==10);
            Assert.Contains(grid, p => p.Values["Fast"]==4 && p.Values["Slow"]==20);
        }
    }
}
