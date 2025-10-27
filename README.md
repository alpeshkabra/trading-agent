# BacktestingEngine (C# / .NET 8)

A minimal yet extensible backtesting skeleton you can extend into a full-fledged engine.

## Features

- Console app, .NET 8
- CSV data loader (OHLCV, daily)
- Simple SMA crossover strategy
- Order/Trade/Portfolio model with daily bar matching
- Execution slippage
- Equity curve & trades CSV reports
- Clean namespaces and folders to extend (Indicators/Strategies/Engine)

## Project layout

(see tree in repo)

## Build & run

Requires .NET 8 SDK.

```bash
cd BacktestingEngine/src
dotnet build -c Release
dotnet run -- --help
```

### Example

```
dotnet run -- --data data/SPY.csv --symbol SPY --from 2015-01-01 --cash 100000 --strategy mac --fast 20 --slow 50 --slippage 0.0005 --report reports
```

Outputs:

- `reports/equity_curve.csv` with Date,Equity
- `reports/trades.csv` with trade blotter
