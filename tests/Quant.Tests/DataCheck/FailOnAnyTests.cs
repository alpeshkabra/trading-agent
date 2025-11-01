using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace Quant.Tests.DataCheck
{
    public class FailOnAnyTests
    {
        [Fact]
        public void NonZeroExit_When_FailOnAny_And_Issues()
        {
            var dir = Directory.CreateTempSubdirectory();
            var data = Path.Combine(dir.FullName, "data.csv");
            var outd = Path.Combine(dir.FullName, "out");

            // Introduce a duplicate & gap
            File.WriteAllText(data,
@"Date,Open,High,Low,Close,Volume
2024-01-02,100,101,99,100,10
2024-01-02,100,101,99,100,10
2024-02-10,100,101,99,100,10
");

            var srcPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src"));
            var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";
            var args = $"run --project \"{srcPath}\" -- data-check --data \"{data}\" --out \"{outd}\" --fail-on any";

            var psi = new ProcessStartInfo(exe, args)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var p = Process.Start(psi)!;
            p.WaitForExit(30_000);

            Assert.NotEqual(0, p.ExitCode);
        }
    }
}
