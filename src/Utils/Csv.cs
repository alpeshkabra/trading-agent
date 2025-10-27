namespace Backtesting.Utils;

public static class Csv
{
    public static IEnumerable<string[]> Read(string path, bool hasHeader = true)
    {
        using var sr = new StreamReader(path);
        string? line;
        if (hasHeader) sr.ReadLine();
        while ((line = sr.ReadLine()) is not null)
        {
            yield return line.Split(',');
        }
    }

    public static void Write(string path, IEnumerable<string[]> rows, string[]? header = null)
    {
        using var sw = new StreamWriter(path);
        if (header is not null) sw.WriteLine(string.Join(",", header));
        foreach (var r in rows) sw.WriteLine(string.Join(",", r.Select(QuoteIfNeeded)));
        static string QuoteIfNeeded(string s) => s.Contains(',') ? $""{s}"" : s;
    }
}
