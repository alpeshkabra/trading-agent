namespace QuantFrameworks.Costs
{
    public interface ITransactionCostModel
    {
        decimal Compute(decimal price, int quantity, string symbol);
    }
}
