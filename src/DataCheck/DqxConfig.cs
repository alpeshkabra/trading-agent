namespace QuantFrameworks.DataCheck
{
    public sealed class DqxConfig
    {
        public int MaxGapDays { get; set; } = 3;
        public double MaxAbsReturn { get; set; } = 0.25; // 25%
        public long MinVolume { get; set; } = 0;
        // none | any | outliers | gaps | duplicates
        public string FailOn { get; set; } = "none";
    }
}
