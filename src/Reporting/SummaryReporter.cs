using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using QuantFrameworks.IO;

namespace QuantFrameworks.Reporting
{
    public sealed class SummaryReport
    {
        public decimal Cash { get; init; }
        public decimal MarketValue { get; init; }
        public decimal NAV => Cash + MarketValue;

        public decimal CostBasis { get; init; }
        public decimal UnrealizedPnL => MarketValue - CostBasis;
        public decimal RealizedPnL { get; init; }

        public decimal DailyReturn { get; init; }
        public decimal Sharpe { get; init; }
        public decimal MaxDrawdown { get; init; }

        public IReadOnlyList<(string Symbol, int Qty, decimal Px, decimal Value)> Lines { get; init; } =
            Array.Empty<(string, int, decimal, decimal)>();
    }

    public static class SummaryReporter
    {
        public static SummaryReport FromSnapshot(
            PortfolioSnapshot portfolio,
            IReadOnlyDictionary<string, decimal> lastPrices,
            decimal commissionPerTrade = 0m)
        {
            decimal mv = 0m, cost = 0m;
            var lines = new List<(string sym, int qty, decimal px, decimal val)>();

            foreach (var pos in portfolio.Positions.Values.OrderBy(p => p.Symbol, StringComparer.OrdinalIgnoreCase))
            {
                lastPrices.TryGetValue(pos.Symbol, out var px);
                var val = px * pos.Quantity;
                mv += val;
                cost += pos.AvgPrice * pos.Quantity;
                lines.Add((pos.Symbol, pos.Quantity, px, val));
            }

            return new SummaryReport
            {
                Cash = portfolio.Cash,
                MarketValue = mv,
                CostBasis = cost,
                RealizedPnL = 0m,
                DailyReturn = 0m,
                Sharpe = 0m,
                MaxDrawdown = 0m,
                Lines = lines
            };
        }
    }

    public static class SummaryReporterWriter
    {
        public static void WriteCsv(SummaryReport rpt, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            using var sw = new StreamWriter(path);
            sw.WriteLine("Key,Value");
            sw.WriteLine($"Cash,{rpt.Cash.ToString(CultureInfo.InvariantCulture)}");
            sw.WriteLine($"MarketValue,{rpt.MarketValue.ToString(CultureInfo.InvariantCulture)}");
            sw.WriteLine($"NAV,{rpt.NAV.ToString(CultureInfo.InvariantCulture)}");
            sw.WriteLine($"CostBasis,{rpt.CostBasis.ToString(CultureInfo.InvariantCulture)}");
            sw.WriteLine($"UnrealizedPnL,{rpt.UnrealizedPnL.ToString(CultureInfo.InvariantCulture)}");
            sw.WriteLine($"RealizedPnL,{rpt.RealizedPnL.ToString(CultureInfo.InvariantCulture)}");
            sw.WriteLine($"DailyReturn,{rpt.DailyReturn.ToString(CultureInfo.InvariantCulture)}");
            sw.WriteLine($"Sharpe,{rpt.Sharpe.ToString(CultureInfo.InvariantCulture)}");
            sw.WriteLine($"MaxDrawdown,{rpt.MaxDrawdown.ToString(CultureInfo.InvariantCulture)}");
            sw.WriteLine();
            sw.WriteLine("Symbol,Quantity,Price,Value");
            foreach (var l in rpt.Lines)
            {
                sw.WriteLine($"{l.Symbol},{l.Qty},{l.Px.ToString(CultureInfo.InvariantCulture)},{l.Value.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        public static void WriteConsole(SummaryReport rpt)
        {
            Console.WriteLine("=== Portfolio Summary ===");
            Console.WriteLine($"Cash           : {rpt.Cash}");
            Console.WriteLine($"Market Value   : {rpt.MarketValue}");
            Console.WriteLine($"NAV            : {rpt.NAV}");
            Console.WriteLine($"Cost Basis     : {rpt.CostBasis}");
            Console.WriteLine($"Unrealized PnL : {rpt.UnrealizedPnL}");
            Console.WriteLine($"Realized PnL   : {rpt.RealizedPnL}");
            Console.WriteLine($"Daily Return   : {rpt.DailyReturn}");
            Console.WriteLine($"Sharpe (toy)   : {rpt.Sharpe}");
            Console.WriteLine($"Max Drawdown   : {rpt.MaxDrawdown}");
            Console.WriteLine();
            Console.WriteLine("Symbol  Qty   Price   Value");
            foreach (var l in rpt.Lines)
                Console.WriteLine($"{l.Symbol,-6} {l.Qty,3}   {l.Px,6}   {l.Value,8}");
        }
    }
}