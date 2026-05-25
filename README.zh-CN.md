# WaterReminder

WaterReminder 是一个轻量的 Windows 桌面托盘应用，用于在可配置的每日时间段内提醒你喝水。

项目基于 .NET 8 和 WPF 构建。应用运行在系统托盘中，到点后显示一个置顶的小型提醒窗口，并将设置保存在当前用户的本地应用数据目录中。

## 功能

- 系统托盘图标和快捷菜单。
- 可从托盘菜单手动触发“立即提醒”。
- 支持配置提醒间隔、开始时间、结束时间、仅工作日提醒、主题模式和开机启动。
- 支持暂停或恢复当天提醒。
- 提醒窗口不会自动消失，必须由用户确认或关闭，避免离开工位时错过提醒。
- 点击“我喝了”、右上角关闭按钮或按 `Esc` 都会确认本次提醒，并从确认时间开始计算下一次提醒。
- 设置和日志均保存在本地，不需要网络访问。

## 环境要求

- Windows
- .NET 8 SDK（用于从源码构建）

## 构建

```powershell
dotnet build -c Release
```

## 发布独立 EXE

```powershell
dotnet publish -c Release -o publish
```

项目已配置为 `win-x64` 自包含单文件发布。生成的可执行文件会输出到 `publish` 目录。

## 本地数据

运行时数据保存在：

```text
%LOCALAPPDATA%\WaterReminder
```

常见文件包括：

- `settings.json`
- `water-reminder.log`

这些文件属于用户本地运行数据，不应提交到源码仓库。

## 项目结构

- `AppController.cs`：协调托盘图标、菜单操作、提醒窗口、设置和调度器。
- `ReminderScheduler.cs`：计算提醒时间，并在用户确认前保持等待状态。
- `NotificationWindow.xaml` 和 `NotificationWindow.xaml.cs`：定义提醒窗口界面和确认行为。
- `SettingsStore.cs`：读取和写入本地 JSON 设置。
- `StartupService.cs`：管理当前用户的 Windows 开机启动注册表项。
- `TrayIconFactory.cs`：在运行时创建托盘图标。

## 贡献说明

- 不要提交 `bin/`、`obj/` 或 `publish/` 中的构建产物。
- 不要提交用户本地设置或日志。
- 发布改动前建议先运行 Release 构建。

## 许可证

本项目使用 MIT 许可证。详情见 [LICENSE](LICENSE)。
