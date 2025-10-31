using System.IO;

namespace QuantFrameworks.Reporting.Tearsheet
{
    public static class TearsheetWriter
    {
        public static void WriteHtml(TearsheetModel model, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            var html = HtmlTearsheetBuilder.Build(model);
            File.WriteAllText(path, html);
        }

        public static void WriteMarkdown(TearsheetModel model, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            var md = MarkdownTearsheetBuilder.Build(model);
            File.WriteAllText(path, md);
        }
    }
}
