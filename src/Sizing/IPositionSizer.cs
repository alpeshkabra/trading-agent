namespace QuantFrameworks.Sizing
{
    public interface IPositionSizer
    {
        /// <summary>Return an absolute quantity for a trade at price given nav.</summary>
        int Size(decimal price, decimal nav);
        int LotSize { get; }
    }
}
