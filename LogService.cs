using System.IO;
using System.Text;

namespace WaterReminder;

public sealed class LogService
{
    private readonly string _directoryPath;
    private readonly string _logPath;
    private readonly object _gate = new();

    public LogService()
    {
        _directoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WaterReminder");
        _logPath = Path.Combine(_directoryPath, "water-reminder.log");
    }

    public void Write(string source, Exception exception)
    {
        Write(source, exception.ToString());
    }

    public void Write(string source, string message)
    {
        lock (_gate)
        {
            Directory.CreateDirectory(_directoryPath);

            var builder = new StringBuilder();
            builder.Append('[')
                .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Append("] [")
                .Append(source)
                .Append("] ")
                .AppendLine(message);

            File.AppendAllText(_logPath, builder.ToString(), Encoding.UTF8);
        }
    }
}
