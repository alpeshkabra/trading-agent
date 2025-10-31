using System;
using QuantFrameworks.Optimize;
using Xunit;

namespace Quant.Tests.Optimize
{
    public class SplitterTests
    {
        [Fact]
        public void KFoldWalkForward_Splits()
        {
            var start = new DateTime(2024,1,1);
            var end   = new DateTime(2024,1,31);
            var folds = Splitter.KFoldWalkForward(start, end, 3, 0.7);
            Assert.True(folds.Count >= 2);
        }
    }
}
