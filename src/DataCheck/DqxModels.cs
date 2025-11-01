using System;

namespace QuantFrameworks.DataCheck
{
    public enum AnomalyKind
    {
        Gap, Duplicate, Unsorted, Outlier, BadRow, ZeroOrNegative
    }

    public sealed class Anomaly
    {
        public DateOnly Date { get; set; }
        public AnomalyKind Kind { get; set; }
        public string Detail { get; set; } = "";
    }

    public sealed class DqxSummary
    {
        public int Rows { get; set; }
        public int Gaps { get; set; }
        public int Duplicates { get; set; }
        public int Unsorted { get; set; }
        public int Outliers { get; set; }
        public int BadRows { get; set; }
        public int ZeroOrNegative { get; set; }
        public int TotalIssues => Gaps + Duplicates + Unsorted + Outliers + BadRows + ZeroOrNegative;
    }
}
