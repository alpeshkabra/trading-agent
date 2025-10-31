using System;
using System.Collections.Generic;

namespace QuantFrameworks.Optimize
{
    public static class Splitter
    {
        public static List<(DateTime trainStart, DateTime trainEnd, DateTime testStart, DateTime testEnd)>
            KFoldWalkForward(DateTime start, DateTime end, int k, double trainRatio)
        {
            var spans = new List<(DateTime,DateTime,DateTime,DateTime)>();
            if (k <= 0) return spans;
            var totalDays = (end - start).Days + 1;
            if (totalDays <= 2) return spans;

            var foldLen = Math.Max(2, totalDays / k);
            for (int i = 0; i < k; i++)
            {
                var fs = start.AddDays(i * foldLen);
                var fe = i == k - 1 ? end : fs.AddDays(foldLen - 1);
                if (fe < fs) break;

                var trainDays = (int)Math.Max(1, (fe - fs).Days * trainRatio);
                var ts = fs;
                var te = fs.AddDays(trainDays);
                if (te >= fe) te = fs.AddDays(Math.Max(1, (fe - fs).Days / 2));

                var vs = te.AddDays(1);
                if (vs > fe) vs = te; // degenerate
                var ve = fe;

                spans.Add((ts, te, vs, ve));
            }
            return spans;
        }
    }
}
