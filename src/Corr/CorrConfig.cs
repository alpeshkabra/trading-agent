namespace QuantFrameworks.Corr
{
    public sealed class CorrConfig
    {
        public int Window { get; set; } = 20;
        public string OutputDir { get; set; } = "out/corr";
    }
}
