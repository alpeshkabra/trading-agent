using System;
using System.Collections.Generic;

namespace QuantFrameworks.Reporting
{
    public sealed class RunReport
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public decimal StartingCash { get; init; }
        public decimal EndingNAV { get; init; }
        public decimal MaxDrawdown { get; set; }
        
        public List<(DateTime Date, decimal NAV)> DailyNav { get; } = new();
    }
}
