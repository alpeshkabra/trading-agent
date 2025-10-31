using System.Collections.Generic;
using System.Linq;

namespace QuantFrameworks.Optimize
{
    public static class GridGenerator
    {
        public static IEnumerable<ParamSet> Cartesian(IEnumerable<ParamSpec> specs)
        {
            var lists = specs.Select(s => ParameterRange.Expand(s).Select(v => (s.Name, v)).ToList()).ToList();
            if (lists.Count == 0) yield break;

            foreach (var tuple in Product(lists))
            {
                var ps = new ParamSet();
                foreach (var (n, v) in tuple) ps.Values[n] = v;
                yield return ps;
            }
        }

        private static IEnumerable<List<(string, int)>> Product(List<List<(string,int)>> lists)
        {
            var idx = new int[lists.Count];
            while (true)
            {
                var current = new List<(string,int)>(lists.Count);
                for (int i = 0; i < lists.Count; i++)
                    current.Add(lists[i][idx[i]]);
                yield return current;

                int k = lists.Count - 1;
                while (k >= 0)
                {
                    idx[k]++;
                    if (idx[k] < lists[k].Count) break;
                    idx[k] = 0; k--;
                }
                if (k < 0) yield break;
            }
        }
    }
}
