using Quant.Models;

namespace Quant.Ledger;

public static class PnlEngine
{
    public static List<DailyRecord> BuildDailySeries(
        IEnumerable<Trade> trades,
        Dictionary<string, Dictionary<DateOnly, double>> prices,
        double initialCash,
        IEnumerable<CashFlow>? cashFlows = null)
    {
        var cfMap = new Dictionary<DateOnly, double>();
        if (cashFlows is not null)
        {
            foreach (var cf in cashFlows)
                cfMap[cf.Date] = cfMap.TryGetValue(cf.Date, out var v) ? v + cf.Amount : cf.Amount;
        }

        var allDates = new SortedSet<DateOnly>();
        foreach (var kv in prices)
            foreach (var d in kv.Value.Keys)
                allDates.Add(d);
        foreach (var t in trades)
            allDates.Add(t.Date);
        if (allDates.Count == 0) return new List<DailyRecord>();

        var tradesByDate = trades.GroupBy(t => t.Date).ToDictionary(g => g.Key, g => g.ToList());

        var qty = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        double cash = initialCash;

        var outList = new List<DailyRecord>(allDates.Count);

        foreach (var date in allDates)
        {
            double flow = 0.0;
            if (cfMap.TryGetValue(date, out var f))
            {
                cash += f;
                flow = f;
            }

            if (tradesByDate.TryGetValue(date, out var todaysTrades))
            {
                foreach (var tr in todaysTrades)
                {
                    if (!prices.TryGetValue(tr.Symbol, out var map) || !map.TryGetValue(date, out var px))
                        throw new InvalidOperationException($"Missing price for {tr.Symbol} on {date:yyyy-MM-dd}");
                    var cost = tr.Quantity * px + tr.Fees;
                    cash -= cost;
                    qty[tr.Symbol] = qty.TryGetValue(tr.Symbol, out var qPrev) ? qPrev + tr.Quantity : tr.Quantity;
                }
            }

            double value = cash;
            foreach (var (sym, q) in qty)
            {
                if (!prices.TryGetValue(sym, out var map) || !map.TryGetValue(date, out var px))
                    continue;
                value += q * px;
            }

            outList.Add(new DailyRecord(date, value, flow));
        }

        return outList;
    }
}
