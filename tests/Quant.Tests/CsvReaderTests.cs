using Quant.Data;
using Quant.Models;

namespace Quant.Tests;

public class CsvReaderTests
{
    [Fact]
    public void ReadsBars_ParsesRows_WithHeader()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, string.Join(Environment.NewLine, new[] {
                "Date,Open,High,Low,Close,Volume",
                "2020-01-02,100,101,99,100.5,123456",
                "2020-01-03,100.5,101,98,99.5,100000"
            }));

            var reader = new CsvReader(path);
            var bars = reader.ReadBars().ToList();

            Assert.Equal(2, bars.Count);
            Assert.Equal(new DateOnly(2020,1,2), bars[0].Date);
            Assert.Equal(100d, bars[0].Open);
            Assert.Equal(100.5d, bars[0].Close);
            Assert.Equal(123456d, bars[0].Volume);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void DateFilters_Work()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, string.Join(Environment.NewLine, new[] {
                "Date,Open,High,Low,Close,Volume",
                "2020-01-02,100,101,99,100.5,123456",
                "2020-01-03,100.5,101,98,99.5,100000",
                "2020-01-06,100.0,101,99,100.0,120000"
            }));

            var reader = new CsvReader(path);
            var from = new DateOnly(2020,1,3);
            var to   = new DateOnly(2020,1,3);
            var bars = reader.ReadBars(from, to).ToList();

            Assert.Single(bars);
            Assert.Equal(new DateOnly(2020,1,3), bars[0].Date);
        }
        finally { File.Delete(path); }
    }
}
