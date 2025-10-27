namespace Quant.Reports
{
    public static class CsvWriter
    {
        public static void Write(string path, IEnumerable<string[]> rows)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            using var sw = new StreamWriter(path);
            foreach (var r in rows)
            {
                sw.WriteLine(string.Join(",", r.Select(Quote)));
            }
        }

        private static string Quote(string s)
        {
            // Quote if it contains comma, quote, or newline; double quotes inside (RFC 4180)
            if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
            {
                var escaped = s.Replace("\"", "\"\"");
                return "\"" + escaped + "\"";
            }
            return s;
        }
    }
}
