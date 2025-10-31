using System.Collections.Generic;

namespace QuantFrameworks.Optimize
{
    internal static class ParameterRange
    {
        public static IEnumerable<int> Expand(ParamSpec s)
        {
            if (s.Values is not null && s.Values.Count > 0)
            {
                foreach (var v in s.Values) yield return v;
                yield break;
            }

            var from = s.From ?? 0;
            var to   = s.To   ?? from;
            var step = s.Step ?? 1;
            if (step <= 0) step = 1;

            if (to < from) yield break;
            for (int v = from; v <= to; v += step)
                yield return v;
        }
    }
}
