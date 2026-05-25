using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;

namespace WaterReminder;

public partial class NotificationWindow : Window
{
    public NotificationWindow(AppSettings settings, DateTime? nextReminderAt)
    {
        InitializeComponent();

        ApplyPalette(ThemeService.Resolve(settings.Theme));
        NextTimeText.Text = nextReminderAt is null
            ? $"确认后，将在 {settings.IntervalMinutes} 分钟后再次提醒。"
            : $"预计下一次提醒时间：{nextReminderAt:HH:mm}";

        Loaded += OnLoaded;
    }

    public event EventHandler? DrinkConfirmed;

    public void Pulse()
    {
        Topmost = false;
        Topmost = true;
        Activate();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 18;
        Top = workArea.Bottom - Height - 18;
        DrinkButton.Focus();
    }

    private void ApplyPalette(ThemePalette palette)
    {
        OuterShell.Background = new SolidColorBrush(MediaColor.FromArgb(235, palette.OuterBackground.R, palette.OuterBackground.G, palette.OuterBackground.B));
        CardBorder.Background = new SolidColorBrush(palette.CardBackground);
        BadgeBorder.Background = new LinearGradientBrush(palette.AccentStart, palette.AccentEnd, 55d);
        ShadowEffect.Color = palette.ShadowColor;

        var foregroundBrush = new SolidColorBrush(palette.Foreground);
        var mutedBrush = new SolidColorBrush(palette.MutedForeground);
        var accentBrush = new LinearGradientBrush(palette.AccentStart, palette.AccentEnd, 0d);
        var surfaceBrush = new SolidColorBrush(MediaColor.FromArgb(40, palette.AccentStart.R, palette.AccentStart.G, palette.AccentStart.B));

        TitleText.Foreground = foregroundBrush;
        SubtitleText.Foreground = mutedBrush;
        BodyText.Foreground = foregroundBrush;
        HintText.Foreground = mutedBrush;
        NextTimeText.Foreground = mutedBrush;

        CloseButton.Background = new SolidColorBrush(MediaColor.FromArgb(228, palette.AccentEnd.R, palette.AccentEnd.G, palette.AccentEnd.B));
        CloseButton.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(210, palette.AccentStart.R, palette.AccentStart.G, palette.AccentStart.B));
        CloseGlyph.Stroke = MediaBrushes.White;

        DrinkButton.Background = accentBrush;
        DrinkButton.Foreground = MediaBrushes.White;
        DrinkButton.BorderBrush = MediaBrushes.Transparent;

        BodyCard.Background = surfaceBrush;
    }

    private void DrinkButton_Click(object sender, RoutedEventArgs e)
    {
        ConfirmAndClose();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        ConfirmAndClose();
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ConfirmAndClose();
        }
    }

    private void ConfirmAndClose()
    {
        DrinkConfirmed?.Invoke(this, EventArgs.Empty);
        Close();
    }
}
