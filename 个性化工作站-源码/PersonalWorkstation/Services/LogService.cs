using System.IO;
using System.Text;

namespace PersonalWorkstation.Services;

/// <summary>本地错误日志，只写在本机 logs 目录，绝不外传。</summary>
public static class LogService
{
    private static readonly object Gate = new();

    public static string LogDir { get; set; } = Path.Combine(AppContext.BaseDirectory, "logs");

    public static void Info(string message) => Write("INFO", message);

    public static void Error(string message, Exception? ex = null)
    {
        var text = ex is null ? message : $"{message}{Environment.NewLine}{ex}";
        Write("ERROR", text);
    }

    private static void Write(string level, string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(LogDir);
                var file = Path.Combine(LogDir, $"log-{DateTime.Now:yyyyMMdd}.txt");
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(file, line, new UTF8Encoding(false));
            }
        }
        catch
        {
            // 日志失败不影响主流程
        }
    }
}