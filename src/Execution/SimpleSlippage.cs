namespace QuantFrameworks.Execution
{
    /// bps slippage: buy => price*(1+bps/10000), sell => price*(1-bps/10000)
    public static class SimpleSlippage
    {
        public static decimal Apply(decimal price, int quantity, decimal slippageBps)
        {
            if (slippageBps <= 0) return price;
            var sign = quantity > 0 ? 1m : -1m;
            return price * (1m + sign * (slippageBps / 10_000m));
        }
    }
}
