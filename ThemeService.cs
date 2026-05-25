using Microsoft.Win32;
using MediaColor = System.Windows.Media.Color;

namespace WaterReminder;

public static class ThemeService
{
    public static ThemePalette Resolve(ThemeMode theme)
    {
        var useDark = theme switch
        {
            ThemeMode.Dark => true,
            ThemeMode.Light => false,
            _ => !IsAppsUseLightTheme()
        };

        return useDark
            ? new ThemePalette(
                MediaColor.FromRgb(24, 30, 42),
                MediaColor.FromRgb(38, 48, 64),
                MediaColor.FromRgb(231, 239, 250),
                MediaColor.FromRgb(160, 177, 197),
                MediaColor.FromRgb(100, 181, 246),
                MediaColor.FromRgb(14, 165, 233),
                MediaColor.FromArgb(80, 0, 0, 0))
            : new ThemePalette(
                MediaColor.FromRgb(245, 249, 255),
                MediaColor.FromRgb(255, 255, 255),
                MediaColor.FromRgb(19, 35, 56),
                MediaColor.FromRgb(87, 104, 124),
                MediaColor.FromRgb(52, 152, 219),
                MediaColor.FromRgb(14, 165, 233),
                MediaColor.FromArgb(40, 15, 23, 42));
    }

    private static bool IsAppsUseLightTheme()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        var value = key?.GetValue("AppsUseLightTheme");
        return value is int intValue ? intValue > 0 : true;
    }
}

public readonly record struct ThemePalette(
    MediaColor OuterBackground,
    MediaColor CardBackground,
    MediaColor Foreground,
    MediaColor MutedForeground,
    MediaColor AccentStart,
    MediaColor AccentEnd,
    MediaColor ShadowColor);
