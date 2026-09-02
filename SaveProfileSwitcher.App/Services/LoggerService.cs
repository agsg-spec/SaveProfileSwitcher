using System.Diagnostics;
using System.Text;

namespace SaveProfileSwitcher.App.Services;

public sealed class LoggerService
{
    private static readonly Lazy<LoggerService> LazyInstance = new(() => new LoggerService());
    private readonly object syncRoot = new();
    private readonly string logFilePath;

    public static LoggerService Instance => LazyInstance.Value;

    private LoggerService()
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SaveProfileSwitcher", "Logs");
        Directory.CreateDirectory(root);
        logFilePath = Path.Combine(root, "app.log");
    }

    public void LogInfo(string message) => Write("INFO", message, null);
    public void LogError(string message, Exception? exception) => Write("ERROR", message, exception);
    public string GetLogDirectory() => Path.GetDirectoryName(logFilePath) ?? string.Empty;

    private void Write(string level, string message, Exception? exception)
    {
        try
        {
            var builder = new StringBuilder();
            builder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            builder.Append(" [");
            builder.Append(level);
            builder.Append("] ");
            builder.AppendLine(message);
            if (exception is not null)
            {
                builder.AppendLine(exception.ToString());
            }
            lock (syncRoot)
            {
                File.AppendAllText(logFilePath, builder.ToString(), Encoding.UTF8);
            }
        }
        catch (Exception error)
        {
            Debug.WriteLine(error);
        }
    }
}
