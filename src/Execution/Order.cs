using System;

namespace QuantFrameworks.Execution
{
    public enum OrderType { Market, Limit }
    public sealed record Order(string Symbol, int Quantity, OrderType Type, decimal? LimitPrice = null, string Tag = "", DateTime? SubmittedAt = null);
    public sealed record Fill(string Symbol, int Quantity, decimal Price, DateTime Time, string Tag = "");
}