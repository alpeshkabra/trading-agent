using System;

namespace QuantFrameworks.Costs
{
    /// Fee = FixedPerOrder + PercentOfNotional * |qty| * price, floored by MinFee.
    public sealed class FixedAndPercentCostModel : ITransactionCostModel
    {
        public decimal FixedPerOrder { get; }
        public decimal PercentOfNotional { get; } // fraction (0.001 = 10 bps)
        public decimal MinFee { get; }

        public FixedAndPercentCostModel(decimal fixedPerOrder, decimal percentOfNotional, decimal minFee = 0m)
        {
            FixedPerOrder = fixedPerOrder < 0 ? 0 : fixedPerOrder;
            PercentOfNotional = percentOfNotional < 0 ? 0 : percentOfNotional;
            MinFee = minFee < 0 ? 0 : minFee;
        }

        public decimal Compute(decimal price, int quantity, string symbol)
        {
            var notional = Math.Abs(quantity) * price;
            var fee = FixedPerOrder + notional * PercentOfNotional;
            return fee < MinFee ? MinFee : fee;
        }
    }
}
