namespace QuantFrameworks.Beta
{
    public static class BetaCli
    {
        // "AAPL=path1,MSFT=path2" -> dict
        public static Dictionary<string,string> ParseSymbols(string spec)
        {
            var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var kv = part.Split('=', 2, StringSplitOptions.TrimEntries);
                if (kv.Length != 2) continue;
                var k = kv[0].ToUpperInvariant();
                var v = kv[1];
                if (!string.IsNullOrWhiteSpace(k) && !string.IsNullOrWhiteSpace(v))
                    dict[k] = v;
            }
            if (dict.Count == 0) throw new ArgumentException("No symbols parsed.");
            return dict;
        }
    }
}
