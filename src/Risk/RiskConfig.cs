using System.Text.Json;

namespace QuantFrameworks.Risk
{
    public sealed class RiskConfig
    {
        public string BaseCurrency { get; set; } = "USD";
        public decimal MaxPerSymbolExposure { get; set; } = 20_000m;
        public decimal MaxAggregateExposure { get; set; } = 100_000m;
        public decimal MaxDailyNotional { get; set; } = 50_000m;
        public HashSet<string> Blacklist { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string,int> PerSymbolMaxQty { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public SizingConfig Sizing { get; set; } = new();

        public static RiskConfig Load(string path)
        {
            var json = File.ReadAllText(path);
            var cfg = JsonSerializer.Deserialize<RiskConfig>(json, new JsonSerializerOptions{ PropertyNameCaseInsensitive = true })
                      ?? new RiskConfig();
            return cfg;
        }
    }

    public sealed class SizingConfig
    {
        public string Mode { get; set; } = "None"; // None | FixedFraction | VolTarget
        public double FixedFraction { get; set; } = 0.1;
        public double VolTargetAnnual { get; set; } = 0.15;
        public int LookbackDays { get; set; } = 20;
        public decimal Capital { get; set; } = 100_000m;
    }
}
