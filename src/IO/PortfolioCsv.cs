using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace QuantFrameworks.IO
{
    public sealed class PortfolioSnapshot
    {
        public sealed class Position
        {
            public string Symbol { get; init; } = "";
            public int Quantity { get; init; }
            public decimal AvgPrice { get; init; }
            public string Currency { get; init; } = "USD";
        }

        public Dictionary<string, Position> Positions { get; } = new(StringComparer.OrdinalIgnoreCase);

        // Make Cash settable so we can assign it when we encounter __CASH__
        public decimal Cash { get; set; }
    }

    public static class PortfolioCsv
    {
        // Schema:
        // Symbol,Quantity,AvgPrice,Currency
        // AAPL,10,100,USD
        // __CASH__,,5000,USD  (cash line uses __CASH__)
        public static PortfolioSnapshot Read(TextReader reader)
        {
            string? line = reader.ReadLine(); // header
            if (line == null) throw new InvalidDataException("Empty portfolio CSV.");

            var snapshot = new PortfolioSnapshot();
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 4) throw new InvalidDataException($"Bad portfolio line: {line}");

                var symbol = parts[0].Trim();
                var qtyStr = parts[1].Trim();
                var avgStr = parts[2].Trim();
                var ccy = parts[3].Trim();

                if (symbol.Equals("__CASH__", StringComparison.OrdinalIgnoreCase))
                {
                    if (!decimal.TryParse(avgStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var cash))
                        cash = 0m;

                    // Just set Cash; do not replace the object or Positions dictionary
                    snapshot.Cash = cash;
                    continue;
                }

                if (!int.TryParse(qtyStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var qty))
                    qty = 0;
                if (!decimal.TryParse(avgStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var avg))
                    avg = 0m;

                snapshot.Positions[symbol] = new PortfolioSnapshot.Position
                {
                    Symbol = symbol,
                    Quantity = qty,
                    AvgPrice = avg,
                    Currency = string.IsNullOrWhiteSpace(ccy) ? "USD" : ccy
                };
            }

            return snapshot;
        }
    }
}
