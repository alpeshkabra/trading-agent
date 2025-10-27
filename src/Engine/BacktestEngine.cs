using Backtesting.Core;
using Backtesting.Models;
using Backtesting.Strategies;
using Backtesting.Utils;

namespace Backtesting.Engine;

public class BacktestEngine
{
    private readonly string _symbol;
    private readonly ExecutionSimulator _exec;

    public BacktestEngine(string symbol, ExecutionSimulator exec)
    {
        _symbol = symbol;
        _exec = exec;
    }

    public (Portfolio portfolio, List<Trade> trades) Run(IReadOnlyList<Bar> bars, IStrategy strategy, double startingCash)
    {
        var portfolio = new Portfolio(startingCash);
        var trades = new List<Trade>();
        strategy.Initialize();

        foreach (var bar in bars)
        {
            foreach (var order in strategy.OnData(bar))
            {
                var trade = _exec.Execute(order, bar);
                if (trade is not null)
                {
                    trades.Add(trade);
                    portfolio.ApplyFill(trade.Date, trade.Side, trade.Quantity, trade.Price);
                }
            }
            portfolio.MarkToMarket(bar.Date, bar.Close);
        }

        strategy.OnEnd();
        Logger.Info($"Completed backtest for {_symbol} with {trades.Count} trades.");
        return (portfolio, trades);
    }
}
