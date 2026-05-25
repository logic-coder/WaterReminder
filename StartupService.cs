using Microsoft.Win32;

namespace WaterReminder;

public sealed class StartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "WaterReminder";
    private readonly LogService _logService;

    public StartupService(LogService logService)
    {
        _logService = logService;
    }

    public bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(AppName) is string;
        }
        catch (Exception exception)
        {
            _logService.Write("AUTOSTART-READ", exception);
            return false;
        }
    }

    public void SetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            if (enabled)
            {
                var processPath = Environment.ProcessPath;
                if (string.IsNullOrWhiteSpace(processPath))
                {
                    return;
                }

                key?.SetValue(AppName, $"\"{processPath}\"");
                return;
            }

            key?.DeleteValue(AppName, false);
        }
        catch (Exception exception)
        {
            _logService.Write("AUTOSTART-WRITE", exception);
        }
    }
}
