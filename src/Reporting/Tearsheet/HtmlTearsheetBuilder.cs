using System.Globalization;
using System.Text;
using QuantFrameworks.Reporting.Charts;

namespace QuantFrameworks.Reporting.Tearsheet
{
    public static class HtmlTearsheetBuilder
    {
        public static string Build(TearsheetModel m)
        {
            string navSpark = SimpleSvgChart.Sparkline(m.DailyNav);
            string ddFill   = SimpleSvgChart.FilledDrawdown(m.DailyNav);

            string F(decimal v) => v.ToString("N2", CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.Append("<!doctype html><html><head><meta charset='utf-8'><title>");
            sb.Append(System.Net.WebUtility.HtmlEncode(m.Title));
            sb.Append("</title><style>");
            sb.Append("body{font-family:Segoe UI, Roboto, Helvetica, Arial, sans-serif;margin:24px;color:#111;}");
            sb.Append(".grid{display:grid;grid-template-columns:1fr 1fr;gap:24px;}");
            sb.Append("h1{margin:0 0 8px 0;} .muted{color:#666;} table{border-collapse:collapse;} td{padding:4px 8px;} tr:nth-child(odd){background:#fafafa;}");
            sb.Append(".card{border:1px solid #eee;border-radius:12px;padding:16px;box-shadow:0 1px 3px rgba(0,0,0,.04);}");
            sb.Append("</style></head><body>");
            sb.Append($"<h1>{System.Net.WebUtility.HtmlEncode(m.Title)}</h1>");
            sb.Append($"<div class='muted'>{m.Start:yyyy-MM-dd} → {m.End:yyyy-MM-dd}</div>");

            sb.Append("<div class='grid'>");
            sb.Append("<div class='card'><h3>NAV</h3>");
            sb.Append(navSpark);
            sb.Append("</div>");
            sb.Append("<div class='card'><h3>Drawdown</h3>");
            sb.Append(ddFill);
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("<div class='card' style='margin-top:24px'><h3>Summary</h3><table>");
            foreach (var kv in m.Summary)
                sb.Append($"<tr><td>{System.Net.WebUtility.HtmlEncode(kv.Key)}</td><td style='text-align:right'>{F(kv.Value)}</td></tr>");
            sb.Append("</table></div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}
