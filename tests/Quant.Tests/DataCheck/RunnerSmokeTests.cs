using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace Quant.Tests.DataCheck
{
    public class RunnerSmokeTests
    {
        [Fact]
        public void DataCheck_Produces_Outputs_And_ZeroExit()
        {
            var dir = Directory.CreateTempSubdirectory();
            var data = Path.Combine(dir.FullName, "data.csv");
            var outd = Path.Combine(dir.FullName, "out");

            File.WriteAllText(data,
@"Date,Open,High,Low,Close,Volume
2024-01-02,100,101,99,100,10
2024-01-03,100,101,99,101,20
2024-01-04,101,102,100,102,30
");

            var srcPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src"));
            var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";
            var args = $"run --project \"{srcPath}\" -- data-check --data \"{data}\" --out \"{outd}\" --fail-on none";

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

            Assert.True(File.Exists(Path.Combine(outd, "report.csv")));
            Assert.True(File.Exists(Path.Combine(outd, "anomalies.csv")));
        }
    }
}
