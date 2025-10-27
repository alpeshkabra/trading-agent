# QuantFrameworks (C# / .NET 8)

Focused starter: read CSV OHLCV, compute daily returns, Quant SPY vs stocks.

## Build
```bash
cd src
dotnet build
```

## Run
```bash
dotnet run -- --help
dotnet run -- --spy data/SPY.csv --stocks data/AAPL.csv,data/MSFT.csv --from 2018-01-01 --out reports
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


## Run tests

dotnet sln Quant.sln add .\tests\Quant.Tests\Quant.Tests.csproj
dotnet test
