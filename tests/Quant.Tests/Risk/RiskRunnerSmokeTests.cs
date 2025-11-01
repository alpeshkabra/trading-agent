using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace Quant.Tests.Risk
{
    public class RiskRunnerSmokeTests
    {
        [Fact]
        public void Produces_Outputs()
        {
            var dir = Directory.CreateTempSubdirectory();
            var orders = Path.Combine(dir.FullName, "orders.csv");
            var cfg    = Path.Combine(dir.FullName, "risk.json");
            var prices = Path.Combine(dir.FullName, "prices.csv");
            var outd   = Path.Combine(dir.FullName, "out");

            File.WriteAllText(orders, "timestamp,symbol,side,qty,price\n2024-01-02T09:30:00Z,ABC,BUY,,100\n");
            File.WriteAllText(cfg, "{\"sizing\":{\"mode\":\"FixedFraction\",\"fixedFraction\":0.1,\"capital\":100000},\"maxPerSymbolExposure\":50000}");
            File.WriteAllText(prices, "date,symbol,close\n2024-01-01,ABC,100\n2024-01-02,ABC,101\n");

            // Resolve the src folder relative to the test binary location:
            // bin/Debug/net8.0  -> up to tests/Quant.Tests -> up to tests -> up to repo root -> src
            var srcPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src"));

            var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";
            var args = $"run --project \"{srcPath}\" -- risk-check --orders \"{orders}\" --config \"{cfg}\" --prices \"{prices}\" --out \"{outd}\"";

            var psi = new ProcessStartInfo(exe, args)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var p = Process.Start(psi)!;
            p.WaitForExit(30_000);

            // Helpful when diagnosing failures locally
            var stdout = p.StandardOutput.ReadToEnd();
            var stderr = p.StandardError.ReadToEnd();
            if (p.ExitCode != 0)
            {
                throw new Xunit.Sdk.XunitException($"dotnet run exit {p.ExitCode}\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
            }

            Assert.Equal(0, p.ExitCode);
            Assert.True(File.Exists(Path.Combine(outd, "risk_report.csv")));
            Assert.True(File.Exists(Path.Combine(outd, "validated_orders.csv")));
        }
    }
}
