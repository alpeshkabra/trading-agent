using System;
using System.Linq;

namespace QuantFrameworks.Reporting.Tearsheet
{
    public static class TearsheetFromRun
    {
        public static TearsheetModel Build(QuantFrameworks.Reporting.SummaryReport summary,
                                           QuantFrameworks.Reporting.RunReport run,
                                           string title = "Backtest Tear Sheet")
        {
            var model = new TearsheetModel
            {
                Title = title,
                Start = run.Start,
                End = run.End,
                StartingCash = run.StartingCash,
                EndingNav = run.EndingNAV,
                Sharpe = summary.Sharpe,
                MaxDrawdown = run.MaxDrawdown
            };

            // Use the actual tuple names: Date, NAV
            foreach (var pt in run.DailyNav.OrderBy(x => x.Date))
            {
                model.Dates.Add(pt.Date);
                model.DailyNav.Add(pt.NAV);
            }

            decimal totalReturn = run.StartingCash != 0m ? (run.EndingNAV / run.StartingCash - 1m) : 0m;

            model.Summary["Starting Cash"] = model.StartingCash;
            model.Summary["Ending NAV"] = model.EndingNav;
            model.Summary["Total Return (%)"] = totalReturn * 100m;
            model.Summary["Sharpe"] = model.Sharpe;
            model.Summary["Max Drawdown (%)"] = model.MaxDrawdown * 100m;

            return model;
        }
    }
}
