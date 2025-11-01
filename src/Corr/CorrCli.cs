namespace QuantFrameworks.Corr
{
    public static class CorrCli
    {
        public static Dictionary<string,string> ParseSymbols(string spec)
        {
            var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var kv = part.Split('=', 2, StringSplitOptions.TrimEntries);
                if (kv.Length != 2) continue;
                var k = kv[0].ToUpperInvariant();
                var v = kv[1];
                if (string.IsNullOrWhiteSpace(k) || string.IsNullOrWhiteSpace(v)) continue;
                dict[k] = v;
            }
            if (dict.Count < 2) throw new ArgumentException("Need at least two symbols for correlation.");
            return dict;
        }
    }
}
