using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace Quant.Tests.Beta
{
    public class BetaRunnerSmokeTests
    {
        [Fact]
        public void Beta_Command_Writes_Outputs()
        {
            var dir = Directory.CreateTempSubdirectory();
            var a = Path.Combine(dir.FullName, "a.csv");
            var b = Path.Combine(dir.FullName, "b.csv");
            var o = Path.Combine(dir.FullName, "out");

            File.WriteAllText(a,
@"Date,Open,High,Low,Close,Volume
2024-01-01,0,0,0,100,0
2024-01-02,0,0,0,103,0
2024-01-03,0,0,0,104,0
2024-01-04,0,0,0,106,0
2024-01-05,0,0,0,108,0
");
            File.WriteAllText(b,
@"Date,Open,High,Low,Close,Volume
2024-01-01,0,0,0,100,0
2024-01-02,0,0,0,102,0
2024-01-03,0,0,0,101,0
2024-01-04,0,0,0,105,0
2024-01-05,0,0,0,107,0
");

            var symSpec = $"AAA={a}";
            var srcPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src"));
            var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";
            var args = $"run --project \"{srcPath}\" -- beta --symbols \"{symSpec}\" --benchmark \"{b}\" --window 3 --out \"{o}\"";

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

            Assert.True(File.Exists(Path.Combine(o, "rolling_beta.csv")));
            Assert.True(File.Exists(Path.Combine(o, "summary_beta.csv")));
        }
    }
}
