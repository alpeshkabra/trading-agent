using System;
using System.Collections.Generic;

namespace QuantFrameworks.Reporting.Tearsheet
{
    public sealed class TearsheetModel
    {
        public string Title { get; init; } = "Run Report";
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public decimal StartingCash { get; init; }
        public decimal EndingNav { get; init; }
        public decimal Sharpe { get; init; }
        public decimal MaxDrawdown { get; init; }

        public List<DateTime> Dates { get; } = new();
        public List<decimal> DailyNav { get; } = new();
        public Dictionary<string, decimal> Summary { get; } = new();
    }
}
