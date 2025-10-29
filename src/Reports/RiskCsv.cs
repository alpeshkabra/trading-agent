using Quant.Models;

namespace Quant.Reports;

public static class RiskCsv
{
    public static void Write(string path, IEnumerable<RiskSnapshot> rows)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        using var sw = new StreamWriter(path);
        sw.WriteLine("Date,Volatility,DownsideDev,VaR,CVar");
        foreach (var r in rows)
            sw.WriteLine($"{r.Date:yyyy-MM-dd},{r.Volatility:F6},{r.DownsideDev:F6},{r.VaR:F6},{r.CVar:F6}");
    }
}
