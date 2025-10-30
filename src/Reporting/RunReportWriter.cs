using System;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace QuantFrameworks.Reporting
{
    public static class RunReportWriter
    {
        public static void WriteJson(RunReport rpt, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            var json = JsonSerializer.Serialize(rpt, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static void WriteDailyCsv(RunReport rpt, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            using var sw = new StreamWriter(path);
            sw.WriteLine("Date,NAV");
            foreach (var (d, nav) in rpt.DailyNav)
                sw.WriteLine($"{d:yyyy-MM-dd},{nav.ToString(CultureInfo.InvariantCulture)}");
        }
    }
}
