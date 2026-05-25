namespace WaterReminder;

public sealed class AppSettings
{
    public bool Enabled { get; set; } = true;

    public TimeOnly StartTime { get; set; } = new(9, 0);

    public TimeOnly EndTime { get; set; } = new(19, 0);

    public int IntervalMinutes { get; set; } = 45;

    public bool WorkdaysOnly { get; set; } = true;

    public bool AutoStart { get; set; }

    public ThemeMode Theme { get; set; } = ThemeMode.Auto;

    public DateTime? LastAcknowledgedAtLocal { get; set; }

    public DateTime? PausedDateLocal { get; set; }

    public void Normalize()
    {
        if (IntervalMinutes < 15)
        {
            IntervalMinutes = 15;
        }

        if (StartTime >= EndTime)
        {
            StartTime = new TimeOnly(9, 0);
            EndTime = new TimeOnly(19, 0);
        }
    }
}

public enum ThemeMode
{
    Auto,
    Light,
    Dark
}
