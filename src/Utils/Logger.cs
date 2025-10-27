namespace Backtesting.Utils;

public enum LogLevel { Debug, Info, Warn, Error }

public static class Logger
{
    public static LogLevel Level { get; set; } = LogLevel.Info;
    private static object _lock = new();

    public static void Debug(string msg) => Log(LogLevel.Debug, msg);
    public static void Info(string msg) => Log(LogLevel.Info, msg);
    public static void Warn(string msg) => Log(LogLevel.Warn, msg);
    public static void Error(string msg) => Log(LogLevel.Error, msg);

    private static void Log(LogLevel level, string msg)
    {
        if (level < Level) return;
        lock (_lock)
        {
            var ts = DateTime.Now.ToString("HH:mm:ss.fff");
            var prefix = level switch
            {
                LogLevel.Debug => "[DBG]",
                LogLevel.Info => "[INF]",
                LogLevel.Warn => "[WRN]",
                LogLevel.Error => "[ERR]",
                _ => "[???]"
            };
            Console.WriteLine($"{ts} {prefix} {msg}");
        }
    }
}
