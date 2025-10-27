using Backtesting.Core;

namespace Backtesting.Data;

public interface IDataProvider
{
    IEnumerable<Bar> LoadBars(DateOnly? from = null, DateOnly? to = null);
}
