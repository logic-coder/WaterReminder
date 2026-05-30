using System.IO;
using Forms = System.Windows.Forms;

namespace WaterReminder;

public sealed class AppController : IDisposable
{
    private readonly SettingsStore _settingsStore;
    private readonly StartupService _startupService;
    private readonly ReminderScheduler _scheduler;
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ContextMenuStrip _menu;
    private readonly Forms.ToolStripMenuItem _pauseTodayMenuItem;
    private readonly Forms.ToolStripMenuItem _autoStartMenuItem;
    private readonly AppSettings _settings;
    private readonly LocalizedText _text;
    private NotificationWindow? _notificationWindow;
    private bool _disposed;
    private bool _suppressAutoStartChanged;

    public AppController(LogService logService)
    {
        LogService = logService;
        _text = LocalizationService.CurrentText;
        _settingsStore = new SettingsStore(logService);
        _startupService = new StartupService(logService);
        _settings = _settingsStore.Load();
        _settings.Normalize();

        _scheduler = new ReminderScheduler(_settings, PersistSettings);
        _scheduler.ReminderDue += OnReminderDue;
        _scheduler.NextReminderChanged += OnNextReminderChanged;

        _pauseTodayMenuItem = new Forms.ToolStripMenuItem();
        _pauseTodayMenuItem.Click += (_, _) => TogglePauseToday();

        _autoStartMenuItem = new Forms.ToolStripMenuItem(_text.AutoStart);
        _autoStartMenuItem.CheckOnClick = true;
        _autoStartMenuItem.CheckedChanged += AutoStartMenuItemOnCheckedChanged;

        var remindNowMenuItem = new Forms.ToolStripMenuItem(_text.RemindNow);
        remindNowMenuItem.Click += (_, _) => _scheduler.TriggerImmediateReminder();

        var openConfigMenuItem = new Forms.ToolStripMenuItem(_text.OpenConfigFile);
        openConfigMenuItem.Click += (_, _) => OpenConfigDirectory();

        var exitMenuItem = new Forms.ToolStripMenuItem(_text.Exit);
        exitMenuItem.Click += (_, _) => System.Windows.Application.Current.Shutdown();

        _menu = new Forms.ContextMenuStrip();
        _menu.Items.Add(remindNowMenuItem);
        _menu.Items.Add(_pauseTodayMenuItem);
        _menu.Items.Add(_autoStartMenuItem);
        _menu.Items.Add(new Forms.ToolStripSeparator());
        _menu.Items.Add(openConfigMenuItem);
        _menu.Items.Add(exitMenuItem);

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = TrayIconFactory.Create(),
            Visible = true,
            ContextMenuStrip = _menu
        };
        _notifyIcon.DoubleClick += (_, _) => _scheduler.TriggerImmediateReminder();

        SyncAutoStart(forceWrite: false);
        PersistSettings();
    }

    public LogService LogService { get; }

    public void Start()
    {
        UpdateMenuState();
        _scheduler.Start();
        UpdateNotifyText(_scheduler.NextReminderAt, _scheduler.IsWaitingForAcknowledgement);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _scheduler.ReminderDue -= OnReminderDue;
        _scheduler.NextReminderChanged -= OnNextReminderChanged;
        _scheduler.Dispose();

        if (_notificationWindow is not null)
        {
            _notificationWindow.DrinkConfirmed -= OnDrinkConfirmed;
            _notificationWindow.Close();
            _notificationWindow = null;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
    }

    private void OnReminderDue(object? sender, EventArgs e)
    {
        if (_notificationWindow is { IsLoaded: true })
        {
            _notificationWindow.Pulse();
            return;
        }

        _notificationWindow = new NotificationWindow(_settings, _scheduler.NextReminderAt, _text);
        _notificationWindow.DrinkConfirmed += OnDrinkConfirmed;
        _notificationWindow.Closed += OnNotificationWindowClosed;
        _notificationWindow.Show();
        _notificationWindow.Activate();
    }

    private void OnDrinkConfirmed(object? sender, EventArgs e)
    {
        _scheduler.AcknowledgeReminder(DateTime.Now);
    }

    private void OnNotificationWindowClosed(object? sender, EventArgs e)
    {
        if (_notificationWindow is null)
        {
            return;
        }

        _notificationWindow.DrinkConfirmed -= OnDrinkConfirmed;
        _notificationWindow.Closed -= OnNotificationWindowClosed;
        _notificationWindow = null;
    }

    private void OnNextReminderChanged(object? sender, DateTime? nextReminderAt)
    {
        UpdateNotifyText(nextReminderAt, _scheduler.IsWaitingForAcknowledgement);
        UpdateMenuState();
    }

    private void TogglePauseToday()
    {
        _scheduler.TogglePauseToday();

        if (_settings.PausedDateLocal?.Date == DateTime.Today && _notificationWindow is not null)
        {
            _notificationWindow.Close();
        }
    }

    private void ToggleAutoStart()
    {
        _settings.AutoStart = _autoStartMenuItem.Checked;
        PersistSettings();
        SyncAutoStart(forceWrite: true);
    }

    private void SyncAutoStart(bool forceWrite)
    {
        var currentState = _startupService.IsEnabled();
        if (forceWrite || currentState != _settings.AutoStart)
        {
            _startupService.SetEnabled(_settings.AutoStart);
        }
    }

    private void PersistSettings()
    {
        _settingsStore.Save(_settings);
        _suppressAutoStartChanged = true;
        _autoStartMenuItem.Checked = _settings.AutoStart;
        _suppressAutoStartChanged = false;
        UpdateMenuState();
    }

    private void UpdateNotifyText(DateTime? nextReminderAt, bool waitingForAcknowledgement)
    {
        var text = waitingForAcknowledgement
            ? _text.TrayWaitingForConfirmation
            : nextReminderAt is null
                ? _text.TrayDisabled
                : _text.TrayNextReminder(nextReminderAt.Value);

        _notifyIcon.Text = text.Length > 63 ? text[..63] : text;
    }

    private void UpdateMenuState()
    {
        var paused = _settings.PausedDateLocal?.Date == DateTime.Today;
        _pauseTodayMenuItem.Text = paused ? _text.ResumeToday : _text.PauseToday;
        _autoStartMenuItem.Checked = _settings.AutoStart;
    }

    private void OpenConfigDirectory()
    {
        var configPath = _settingsStore.SettingsPath;
        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
        _ = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = Path.GetDirectoryName(configPath)!,
            UseShellExecute = true
        });
    }

    private void AutoStartMenuItemOnCheckedChanged(object? sender, EventArgs e)
    {
        if (_suppressAutoStartChanged)
        {
            return;
        }

        ToggleAutoStart();
    }
}
