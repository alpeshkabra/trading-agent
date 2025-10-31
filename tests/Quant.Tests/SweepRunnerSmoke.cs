using System;
using System.IO;
using QuantFrameworks.Optimize;
using Xunit;

namespace Quant.Tests.Optimize
{
    public class SweepRunnerSmoke
    {
        [Fact]
        public void Sweep_Runs()
        {
            var a = "Date,Open,High,Low,Close,Volume\n2024-01-01,100,100,100,100,1\n2024-01-02,101,101,101,101,1\n";
            var pa = Path.GetTempFileName(); File.WriteAllText(pa, a);
            var cfg = new OptimizerConfig
            {
                BaseBacktest = new QuantFrameworks.Backtest.BacktestConfig
                {
                    Symbol = "AAPL",
                    DataPath = pa,
                    Start = new DateTime(2024,1,1),
                    End = new DateTime(2024,1,2),
                    StartingCash = 100_000m,
                    Fast = 1, Slow = 2
                },
                Parameters = new(){ new ParamSpec{ Name="Fast", Values=new(){1,2}}, new ParamSpec{ Name="Slow", Values=new(){2,3}} },
                TargetMetric = "NAV",
                MaxDegreeOfParallelism = 2,
                OutputDir = Path.Combine(Path.GetTempPath(), $"opt_{Guid.NewGuid():N}")
            };
            var r = new SweepRunner(cfg).Run();
            Assert.True(r.Runs.Count >= 1);
        }
    }
}
