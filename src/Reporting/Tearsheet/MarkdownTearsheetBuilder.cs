using System.Globalization;
using System.Text;

namespace QuantFrameworks.Reporting.Tearsheet
{
    public static class MarkdownTearsheetBuilder
    {
        public static string Build(TearsheetModel m)
        {
            string F(decimal v) => v.ToString("N2", CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.AppendLine($"# {m.Title}");
            sb.AppendLine();
            sb.AppendLine($"**Period:** {m.Start:yyyy-MM-dd} → {m.End:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine("| Metric | Value |");
            sb.AppendLine("|---|---:|");
            foreach (var kv in m.Summary)
                sb.AppendLine($"| {kv.Key} | {F(kv.Value)} |");
            sb.AppendLine();
            sb.AppendLine("> Charts are available in the HTML tear sheet output.");
            return sb.ToString();
        }
    }
}
