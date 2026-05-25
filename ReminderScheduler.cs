using System.Windows.Threading;
using Microsoft.Win32;

namespace WaterReminder;

public sealed class ReminderScheduler : IDisposable
{
    private readonly AppSettings _settings;
    private readonly Action _persistState;
    private readonly DispatcherTimer _timer;
    private bool _disposed;
    private bool _waitingForAcknowledgement;

    public ReminderScheduler(AppSettings settings, Action persistState)
    {
        _settings = settings;
        _persistState = persistState;
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _timer.Tick += OnTick;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        SystemEvents.TimeChanged += OnTimeChanged;
    }

    public event EventHandler? ReminderDue;

    public event EventHandler<DateTime?>? NextReminderChanged;

    public DateTime? NextReminderAt { get; private set; }

    public bool IsWaitingForAcknowledgement => _waitingForAcknowledgement;

    public void Start()
    {
        NormalizeState(DateTime.Now);
        Recalculate(DateTime.Now);
        _timer.Start();
        Evaluate(DateTime.Now);
    }

    public void TriggerImmediateReminder()
    {
        if (_waitingForAcknowledgement)
        {
            ReminderDue?.Invoke(this, EventArgs.Empty);
            return;
        }

        _waitingForAcknowledgement = true;
        NextReminderAt = null;
        ReminderDue?.Invoke(this, EventArgs.Empty);
        NextReminderChanged?.Invoke(this, NextReminderAt);
    }

    public void AcknowledgeReminder(DateTime acknowledgedAt)
    {
        _waitingForAcknowledgement = false;
        _settings.LastAcknowledgedAtLocal = acknowledgedAt;
        _settings.PausedDateLocal = null;
        _persistState();
        Recalculate(acknowledgedAt);
    }

    public void TogglePauseToday()
    {
        _waitingForAcknowledgement = false;
        _settings.PausedDateLocal = _settings.PausedDateLocal?.Date == DateTime.Today
            ? null
            : DateTime.Today;
        _persistState();
        Recalculate(DateTime.Now);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _timer.Stop();
        _timer.Tick -= OnTick;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        SystemEvents.TimeChanged -= OnTimeChanged;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        Evaluate(DateTime.Now);
    }

    private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode != PowerModes.Resume)
        {
            return;
        }

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (!_waitingForAcknowledgement)
            {
                Recalculate(DateTime.Now);
            }
        });
    }

    private void OnTimeChanged(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (!_waitingForAcknowledgement)
            {
                Recalculate(DateTime.Now);
            }
        });
    }

    private void Evaluate(DateTime now)
    {
        NormalizeState(now);

        if (_waitingForAcknowledgement)
        {
            return;
        }

        if (NextReminderAt is null)
        {
            Recalculate(now);
            return;
        }

        if (now >= NextReminderAt.Value)
        {
            _waitingForAcknowledgement = true;
            NextReminderAt = null;
            ReminderDue?.Invoke(this, EventArgs.Empty);
            NextReminderChanged?.Invoke(this, NextReminderAt);
        }
    }

    private void NormalizeState(DateTime now)
    {
        var changed = false;

        if (_settings.LastAcknowledgedAtLocal?.Date < now.Date)
        {
            _settings.LastAcknowledgedAtLocal = null;
            changed = true;
        }

        if (_settings.PausedDateLocal?.Date < now.Date)
        {
            _settings.PausedDateLocal = null;
            changed = true;
        }

        if (changed)
        {
            _persistState();
        }
    }

    private void Recalculate(DateTime now)
    {
        NextReminderAt = CalculateNextReminder(now);
        NextReminderChanged?.Invoke(this, NextReminderAt);
    }

    private DateTime? CalculateNextReminder(DateTime now)
    {
        if (!_settings.Enabled)
        {
            return null;
        }

        var currentDate = now.Date;
        if (IsPaused(currentDate))
        {
            return FindNextWindowStart(currentDate.AddDays(1));
        }

        if (!IsReminderDay(currentDate))
        {
            return FindNextWindowStart(currentDate.AddDays(1));
        }

        var windowStart = currentDate + _settings.StartTime.ToTimeSpan();
        var windowEnd = currentDate + _settings.EndTime.ToTimeSpan();
        if (now < windowStart)
        {
            return windowStart;
        }

        if (now >= windowEnd)
        {
            return FindNextWindowStart(currentDate.AddDays(1));
        }

        if (_settings.LastAcknowledgedAtLocal is DateTime lastAck &&
            lastAck.Date == currentDate &&
            lastAck >= windowStart &&
            lastAck < windowEnd)
        {
            var acknowledgedCandidate = lastAck.AddMinutes(_settings.IntervalMinutes);
            if (acknowledgedCandidate >= windowEnd)
            {
                return FindNextWindowStart(currentDate.AddDays(1));
            }

            return acknowledgedCandidate <= now ? now : acknowledgedCandidate;
        }

        var minutesFromStart = (now - windowStart).TotalMinutes;
        var slotIndex = (int)Math.Ceiling(minutesFromStart / _settings.IntervalMinutes);
        var slotCandidate = windowStart.AddMinutes(slotIndex * _settings.IntervalMinutes);

        if (slotCandidate >= windowEnd)
        {
            return FindNextWindowStart(currentDate.AddDays(1));
        }

        return slotCandidate;
    }

    private DateTime? FindNextWindowStart(DateTime startDate)
    {
        for (var offset = 0; offset < 14; offset++)
        {
            var date = startDate.Date.AddDays(offset);
            if (!IsReminderDay(date) || IsPaused(date))
            {
                continue;
            }

            return date + _settings.StartTime.ToTimeSpan();
        }

        return null;
    }

    private bool IsPaused(DateTime date)
    {
        return _settings.PausedDateLocal?.Date == date.Date;
    }

    private bool IsReminderDay(DateTime date)
    {
        if (!_settings.WorkdaysOnly)
        {
            return true;
        }

        return date.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday;
    }
}
