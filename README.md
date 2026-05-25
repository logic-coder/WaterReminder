# WaterReminder

[简体中文](README.zh-CN.md)

WaterReminder is a small Windows desktop tray app that reminds you to drink water during a configurable daily time window.

The app is built with WPF on .NET 8. It runs from the system tray, shows a compact topmost reminder window, and stores settings locally under the current user's application data folder.

## Features

- Tray icon with quick actions.
- Manual "remind now" action from the tray menu.
- Configurable reminder interval, start time, end time, workday-only mode, theme mode, and auto-start setting.
- Pause or resume reminders for the current day.
- Reminder window stays visible until the user confirms or closes it, so reminders are not silently missed.
- Clicking "I drank", the close button, or pressing `Esc` confirms the reminder and starts the next interval from that time.
- Local JSON settings and local log file. No network access is required.

## Requirements

- Windows
- .NET 8 SDK for building from source

## Build

```powershell
dotnet build -c Release
```

## Publish A Standalone EXE

```powershell
dotnet publish -c Release -o publish
```

The project is configured for a self-contained `win-x64` single-file publish. The generated executable is written to the `publish` directory.

## Local Data

Runtime data is stored under:

```text
%LOCALAPPDATA%\WaterReminder
```

Typical files include:

- `settings.json`
- `water-reminder.log`

These files are user-local runtime data and are not part of the source repository.

## Project Structure

- `AppController.cs` coordinates the tray icon, menu actions, reminder window, settings, and scheduler.
- `ReminderScheduler.cs` calculates reminder times and waits for user confirmation before scheduling the next interval.
- `NotificationWindow.xaml` and `NotificationWindow.xaml.cs` define the reminder window UI and confirmation behavior.
- `SettingsStore.cs` reads and writes local JSON settings.
- `StartupService.cs` manages the current-user Windows startup registry entry.
- `TrayIconFactory.cs` creates the tray icon at runtime.

## Notes For Contributors

- Do not commit build outputs from `bin/`, `obj/`, or `publish/`.
- Do not commit user-local settings or logs.
- Run a Release build before publishing changes.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
