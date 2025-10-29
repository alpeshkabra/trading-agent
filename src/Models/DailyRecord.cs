namespace Quant.Models;

public readonly record struct DailyRecord(DateOnly Date, double Value, double ExternalFlow);
