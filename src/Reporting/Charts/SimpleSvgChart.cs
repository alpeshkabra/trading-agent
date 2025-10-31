using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace QuantFrameworks.Reporting.Charts
{
    public static class SimpleSvgChart
    {
        public static string Sparkline(IEnumerable<decimal> values, int width = 600, int height = 120, int margin = 6)
        {
            var arr = values?.ToArray() ?? Array.Empty<decimal>();
            if (arr.Length == 0) return Empty(width, height, "No data");

            var min = (double)arr.Min();
            var max = (double)arr.Max();
            if (Math.Abs(max - min) < 1e-12) max = min + 1.0;

            double xStep = (width - 2.0 * margin) / Math.Max(1, arr.Length - 1);
            Func<double, double> scaleY = v => height - margin - (v - min) / (max - min) * (height - 2.0 * margin);

            var sb = new StringBuilder();
            sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}'>");
            sb.Append("<rect x='0' y='0' width='100%' height='100%' fill='white'/>");
            sb.Append($"<text x='{margin}' y='{margin+12}' font-size='12' fill='#333'>min {min:F2} • max {max:F2}</text>");
            sb.Append("<polyline fill='none' stroke='#1f77b4' stroke-width='2' points='");
            for (int i = 0; i < arr.Length; i++)
            {
                double x = margin + i * xStep;
                double y = scaleY((double)arr[i]);
                sb.Append($"{x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)} ");
            }
            sb.Append("'/>");
            sb.Append("</svg>");
            return sb.ToString();
        }

        public static string FilledDrawdown(IEnumerable<decimal> values, int width = 600, int height = 120, int margin = 6)
        {
            var arr = values?.ToArray() ?? Array.Empty<decimal>();
            if (arr.Length == 0) return Empty(width, height, "No data");

            decimal peak = 0m;
            var dd = new double[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > peak) peak = arr[i];
                dd[i] = peak == 0m ? 0.0 : (double)((peak - arr[i]) / peak);
            }
            double maxDD = dd.Length > 0 ? dd.Max() : 0.0;
            if (maxDD <= 1e-12) maxDD = 1.0;

            double xStep = (width - 2.0 * margin) / Math.Max(1, arr.Length - 1);
            Func<double, double> scaleY = v => margin + v / maxDD * (height - 2.0 * margin);

            var sb = new StringBuilder();
            sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}'>");
            sb.Append("<rect x='0' y='0' width='100%' height='100%' fill='white'/>");
            sb.Append($"<text x='{margin}' y='{margin+12}' font-size='12' fill='#333'>max DD {maxDD:P1}</text>");
            sb.Append("<path d='");
            sb.Append($"{margin},{height - margin} ");
            for (int i = 0; i < dd.Length; i++)
            {
                double x = margin + i * xStep;
                double y = scaleY(dd[i]);
                sb.Append($"L {x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)} ");
            }
            sb.Append($"L {margin + (dd.Length-1) * xStep},{height - margin} Z' fill='#ff7f0e' fill-opacity='0.35' stroke='none'/>");
            sb.Append("</svg>");
            return sb.ToString();
        }

        private static string Empty(int width, int height, string label)
        {
            return $"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}'>" +
                   "<rect width='100%' height='100%' fill='white'/>" +
                   $"<text x='{width/2}' y='{height/2}' font-size='14' text-anchor='middle' fill='#666'>{label}</text></svg>";
        }
    }
}
