# QuantFrameworks (C# / .NET 8)

Focused starter: read CSV OHLCV, compute daily returns, Quant SPY vs stocks.

---

## Build

```bash
cd src
dotnet build
```

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

*(unchanged section omitted for brevity)*

---

# ⚙️ Feature: Risk Metrics (Volatility, Downside Deviation, VaR/ES) + Rolling Windows

*(unchanged section omitted for brevity)*

---

# 🚀 Feature: Backtest MVP (SMA Cross over CSV)

*(same content as before, includes Stop-Loss / Take-Profit)*

---

# 💹 **New Feature: Multi-Asset Backtesting + Slippage + Transaction Costs + Daily NAV + JSON Report**

### Overview

A multi-symbol backtesting engine supporting:

* Portfolio-level execution on a **shared time clock**.
* Configurable **slippage (bps)** and **transaction costs**.
* **Daily NAV** computation with Max Drawdown and JSON export.

### CLI

```bash
trading-agent backtest --config examples/configs/multi.json
# or
dotnet run --project src -- backtest --config examples/configs/multi.json
```

### Example Config

File: `examples/configs/multi.json`

```json
{
  "Symbols": ["AAPL", "MSFT"],
  "SymbolData": {
    "AAPL": "examples/data/AAPL.csv",
    "MSFT": "examples/data/MSFT.csv"
  },
  "Start": "2024-01-01",
  "End": "2024-01-05",
  "StartingCash": 100000,
  "Fast": 2,
  "Slow": 3,
  "StopLossPct": 0.02,
  "TakeProfitPct": 0.05,
  "CommissionPerOrder": 0.50,
  "PercentFee": 0.001,
  "MinFee": 0.10,
  "SlippageBps": 25,
  "OutputPath": "out/summary.csv",
  "DailyNavCsv": "out/daily_nav.csv",
  "RunJson": "out/run.json"
}
```

### Example CSVs

**examples/data/AAPL.csv**

```
Date,Open,High,Low,Close,Volume
2024-01-01,100,101,99,100,100000
2024-01-02,101,103,100,102,100000
2024-01-03,102,104,101,103,100000
2024-01-04,103,103,100,101,100000
2024-01-05,101,102,98,99,100000
```

**examples/data/MSFT.csv**

```
Date,Open,High,Low,Close,Volume
2024-01-01,200,201,199,200,100000
2024-01-02,201,203,200,202,100000
2024-01-03,202,204,201,203,100000
2024-01-04,203,203,200,201,100000
2024-01-05,201,202,198,199,100000
```

### Output

* Console summary of portfolio NAV, PnL, Max Drawdown
* CSV: `out/summary.csv`
* Daily NAV: `out/daily_nav.csv`
* JSON report: `out/run.json`

### Key Config Parameters

| Field                     | Description                                 |
| ------------------------- | ------------------------------------------- |
| `Symbols` / `SymbolData`  | Multi-symbol setup (symbol → CSV path)      |
| `SlippageBps`             | Basis-point price impact per trade          |
| `CommissionPerOrder`      | Fixed fee per executed fill                 |
| `PercentFee`              | Fraction of notional (e.g., 0.001 = 10 bps) |
| `MinFee`                  | Minimum fee per fill                        |
| `DailyNavCsv` / `RunJson` | Extra output files for analytics            |

### Components Added

| Module                              | Description                                                |
| ----------------------------------- | ---------------------------------------------------------- |
| `Feeds/MultiCsvMarketDataFeed`      | Merge multiple per-symbol CSVs on a shared time clock      |
| `Strategy/SmaCrossMultiStrategy`    | Routes bars to per-symbol SMA strategies                   |
| `Execution/SimpleSlippage`          | Adds configurable price impact (bps)                       |
| `Costs/FixedAndPercentCostModel`    | Handles commissions and percent-based fees                 |
| `Backtest/MultiAssetBacktestRunner` | Core orchestrator for multi-asset simulation               |
| `Reporting/PerformanceSeries`       | Computes normalized wealth & drawdowns                     |
| `Reporting/RunReport` + `Writer`    | Exports JSON + CSV daily NAV reports                       |
| `tests/Quant.Tests/Backtest/`       | Unit + E2E coverage for feeds, slippage, fees, performance |

### Example Run Output

```
Backtest config:
  Symbols       : AAPL,MSFT
  Fast/Slow SMA : 2/3
  Slippage (bps): 25
  Commission/Fill : 0.5
  Percent Fee     : 0.10%
  Min Fee         : 0.1
=== Portfolio Summary ===
Cash           : 99985
Market Value   : 10500
NAV            : 110485
Cost Basis     : 10000
Unrealized PnL : 485
Max Drawdown   : 3.20%
```

### Tests

```bash
dotnet test
```
