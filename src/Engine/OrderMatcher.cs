using Backtesting.Core;
using Backtesting.Models;

namespace Backtesting.Engine;

public static class OrderMatcher
{
    public static Trade? TryMatch(Order order, Bar bar, double slippage)
    {
        if (order.Type == OrderType.Market)
        {
            var price = ApplySlippage(bar.Open, order.Side, slippage);
            return new Trade { Date = bar.Date, Side = order.Side, Quantity = order.Quantity, Price = price, Tag = order.Tag };
        }
        else
        {
            if (order.LimitPrice is null) return null;
            var lmt = order.LimitPrice.Value;
            if (order.Side == OrderSide.Buy)
            {
                if (bar.Low <= lmt)
                {
                    var fill = Math.Min(lmt, bar.Open);
                    var price = ApplySlippage(fill, order.Side, slippage);
                    return new Trade { Date = bar.Date, Side = order.Side, Quantity = order.Quantity, Price = price, Tag = order.Tag };
                }
            }
            else
            {
                if (bar.High >= lmt)
                {
                    var fill = Math.Max(lmt, bar.Open);
                    var price = ApplySlippage(fill, order.Side, slippage);
                    return new Trade { Date = bar.Date, Side = order.Side, Quantity = order.Quantity, Price = price, Tag = order.Tag };
                }
            }
        }
        return null;
    }

    private static double ApplySlippage(double price, OrderSide side, double slippage)
    {
        if (slippage <= 0) return price;
        return side == OrderSide.Buy ? price * (1 + slippage) : price * (1 - slippage);
    }
}
