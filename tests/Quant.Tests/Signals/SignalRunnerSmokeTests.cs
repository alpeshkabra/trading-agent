using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace Quant.Tests.Signals
{
    public class SignalRunnerSmokeTests
    {
        [Fact]
        public void Signal_Command_Writes_Outputs()
        {
            var dir = Directory.CreateTempSubdirectory();
            var data = Path.Combine(dir.FullName, "data.csv");
            var outd = Path.Combine(dir.FullName, "out");

            File.WriteAllText(data,
@"Date,Open,High,Low,Close,Volume
2024-01-02,100,101,99,100,10
2024-01-03,100,101,99,101,10
2024-01-04,100,101,99,99,10
2024-01-05,100,101,99,102,10
");

            var srcPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src"));
            var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";
            var args = $"run --project \"{srcPath}\" -- signal --data \"{data}\" --out \"{outd}\" --sma-fast 2 --sma-slow 3 --rsi 14 --bb 3 --bb-std 2 --macd-fast 12 --macd-slow 26 --macd-signal 9";

            var psi = new ProcessStartInfo(exe, args)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var p = Process.Start(psi)!;
            p.WaitForExit(30_000);

            if (p.ExitCode != 0)
            {
                var stdout = p.StandardOutput.ReadToEnd();
                var stderr = p.StandardError.ReadToEnd();
                throw new Xunit.Sdk.XunitException($"exit {p.ExitCode}\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
            }

            Assert.True(File.Exists(Path.Combine(outd, "indicators.csv")));
            Assert.True(File.Exists(Path.Combine(outd, "signals.csv")));
        }
    }
}
