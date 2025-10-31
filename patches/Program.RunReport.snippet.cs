// In Main(string[] args) add this before other subcommands you want lower priority:
// if (string.Equals(args[0], "report", StringComparison.OrdinalIgnoreCase)) { return RunReport(args); }

private static int RunReport(string[] args)
{
    // Args: --summary <path to summary csv/json> --run <path to run.json> --out <html path>
    // Optional: --md <md path> --title "My Tear Sheet"
    string? summaryPath = GetArg(args, "summary");
    string? runPath = GetArg(args, "run");
    string? outHtml = GetArg(args, "out");
    string? outMd = GetArg(args, "md");
    string title = GetArg(args, "title", "Backtest Tear Sheet") ?? "Backtest Tear Sheet";

    if (string.IsNullOrWhiteSpace(summaryPath) || string.IsNullOrWhiteSpace(runPath) || string.IsNullOrWhiteSpace(outHtml))
    {
        Console.Error.WriteLine("ERROR: report requires --summary <path> --run <path> --out <out.html>");
        return 2;
    }

    try
    {
        // NOTE: These loaders are assumed to exist in your Reporting module.
        // If not, we can add minimal readers—tell me if you need that.
        var summary = QuantFrameworks.Reporting.SummaryReporter.Load(summaryPath);
        var run     = QuantFrameworks.Reporting.RunReportWriter.Load(runPath);

        var model = QuantFrameworks.Reporting.Tearsheet.TearsheetFromRun.Build(summary, run, title);

        QuantFrameworks.Reporting.Tearsheet.TearsheetWriter.WriteHtml(model, outHtml);
        if (!string.IsNullOrWhiteSpace(outMd))
            QuantFrameworks.Reporting.Tearsheet.TearsheetWriter.WriteMarkdown(model, outMd);

        Console.WriteLine($"Saved tear sheet: {System.IO.Path.GetFullPath(outHtml)}");
        if (!string.IsNullOrWhiteSpace(outMd))
            Console.WriteLine($"Saved markdown  : {System.IO.Path.GetFullPath(outMd)}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("Report failed: " + ex.Message);
        return 1;
    }
}
