using System.IO;
using System.Text.Json;

namespace QuantFrameworks.Optimize
{
    public static class OptimizeJsonWriter
    {
        public static void WriteSweep(SweepResult r, string outDir)
        {
            Directory.CreateDirectory(outDir);
            var path = Path.Combine(outDir, "sweep_results.json");
            File.WriteAllText(path, JsonSerializer.Serialize(r, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static void WriteWfo(WfoResult r, string outDir)
        {
            Directory.CreateDirectory(outDir);
            var path = Path.Combine(outDir, "wfo_results.json");
            File.WriteAllText(path, JsonSerializer.Serialize(r, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
