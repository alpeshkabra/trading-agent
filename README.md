# QuantFrameworks (C# / .NET 8)

Focused starter: read CSV OHLCV, compute daily returns, Quant SPY vs stocks.

---

## Build
```bash
cd src
dotnet build
````

## Run

```bash
dotnet run -- --help
dotnet run -- --spy data/SPY.csv --stocks data/AAPL.csv,data/MSFT.csv --from 2018-01-01 --out reports
dotnet run -- --spy data/SPY.csv --stocks data/AAPL.csv,data/MSFT.csv --from 2020-01-01 --out reports --portfolio AAPL=0.6,MSFT=0.4 --portfolio-label tech6040
```

### CSV format

Header required:

```
Date,Open,High,Low,Close,Volume
2018-01-02,100,101,99,100.5,123456
...
```

## Output

`reports/Quant_<TICKER>_vs_SPY.csv` with columns:
`Date, SPY_Return, <TICKER>_Return, <TICKER>_Excess`

Also prints mean returns and correlation to the console.

---

## Run tests

```bash
dotnet sln Quant.sln add .\tests\Quant.Tests\Quant.Tests.csproj
dotnet test
```

---

# 📈 Feature: Cash Flows (TWR/MWR/IRR) + Trade Ledger → Daily Portfolio

Adds:

* `Quant.Models.CashFlow`, `Quant.Models.DailyRecord`
* `Quant.Analytics.PerformancePlus`:

  * `TimeWeightedReturns(records)` → daily TWR list + linked total
  * `IRR(flows, terminalDate, terminalValue)` → annualized MWR/IRR (365-day basis)
* `Quant.Ledger`:

  * `Trade`, `PositionSnapshot`
  * `PnlEngine.BuildDailySeries(trades, prices, initialCash, cashFlows)` → `List<DailyRecord>`
* `Quant.Reports.DailyRecordCsv` → write Date,Value,ExternalFlow

---

# ⚙️ Feature: Risk Metrics (Volatility, Downside Deviation, VaR/ES) + Rolling Windows

Adds:

* `Quant.Analytics.RiskMetrics` with:

  * `Volatility(returns)` sample std dev
  * `DownsideDeviation(returns, mar=0)`
  * `VaR(returns, alpha)` → **positive loss**
  * `CVar(returns, alpha)` → Expected Shortfall (positive loss)
  * `Rolling(series, window, alpha)` → `List<RiskSnapshot>` per window end
* `Quant.Models.RiskSnapshot`
* `Quant.Reports.RiskCsv`

Example:

```csharp
var snaps = RiskMetrics.Rolling(spyReturns, 252, 0.05);
RiskCsv.Write(Path.Combine("reports", "risk_spy_252.csv"), snaps);
```

---

# 🚀 **New Feature: Backtest MVP (SMA Cross over CSV)**

### Overview

A complete end-to-end backtest engine that reads CSV price data, runs a Simple Moving Average (SMA) crossover strategy, simulates trades via a simple broker, maintains a portfolio, and produces a summary report.

### CLI

```bash
trading-agent backtest --config examples/configs/sma.json
# or
dotnet run --project src/QuantFrameworks -- backtest --config examples/configs/sma.json
```

### Example Config

File: `examples/configs/sma.json`

```json
{
  "DataPath": "examples/data/prices.csv",
  "Symbol": "AAPL",
  "Start": "2024-01-01",
  "End": "2024-01-10",
  "StartingCash": 100000,
  "Fast": 2,
  "Slow": 3,
  "OutputPath": "out/summary.csv"
}
```

### CSV Input

File: `examples/data/prices.csv`

```
Date,Symbol,Open,High,Low,Close,Volume
2024-01-01,AAPL,100,101,99,100,1000000
2024-01-02,AAPL,101,103,100,102,1000000
2024-01-03,AAPL,102,104,101,103,1000000
2024-01-04,AAPL,103,103,100,101,1000000
2024-01-05,AAPL,101,102,98,99,1000000
```

### Output

* Console summary (Cash, Market Value, NAV, PnL)
* `out/summary.csv` generated via `SummaryReporterWriter`

### Components Added

| Module                        | Description                                                     |
| ----------------------------- | --------------------------------------------------------------- |
| `Feeds/`                      | `CsvMarketDataFeed` for OHLCV streaming                         |
| `Strategy/`                   | `SmaCrossStrategy` – fast vs slow crossover                     |
| `Execution/`                  | `SimpleBrokerSimulator` – market & limit fills                  |
| `Portfolio/`                  | `PortfolioState`, `Position` for holdings & cash                |
| `Backtest/`                   | `BacktestConfig`, `BacktestRunner` orchestrating loop           |
| `IO/`                         | `PortfolioCsv`, `PriceCsv` readers                              |
| `Reporting/`                  | `SummaryReporter` & `Writer` for NAV and PnL summaries          |
| `tests/Quant.Tests/Backtest/` | 7 test files covering feed, strategy, fills, portfolio, and E2E |

### Example Run Output

```
=== Portfolio Summary ===
Cash           : 99988
Market Value   : 10200
NAV            : 110188
Cost Basis     : 10000
Unrealized PnL : 200
Realized PnL   : 0
Daily Return   : 0
Sharpe (toy)   : 0
Max Drawdown   : 0
```

### Test

```bash
dotnet test
```
