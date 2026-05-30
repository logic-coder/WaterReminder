using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Media;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;

namespace WaterReminder;

public partial class NotificationWindow : Window
{
    public NotificationWindow(AppSettings settings, DateTime? nextReminderAt, LocalizedText text)
    {
        InitializeComponent();

        ApplyText(settings, nextReminderAt, text);
        ApplyPalette(ThemeService.Resolve(settings.Theme));

        Loaded += OnLoaded;
    }

    public event EventHandler? DrinkConfirmed;

    public void Pulse()
    {
        Topmost = false;
        Topmost = true;
        Activate();
    }

    private void ApplyText(AppSettings settings, DateTime? nextReminderAt, LocalizedText text)
    {
        Title = text.AppTitle;
        TitleText.Text = text.AppTitle;
        SubtitleText.Text = text.WindowSubtitle;
        BodyText.Text = text.WindowBody;
        HintText.Text = text.WindowHint;
        CloseButton.ToolTip = text.CloseReminder;
        AutomationProperties.SetName(CloseButton, text.CloseReminder);
        DrinkButton.Content = text.DrinkConfirmedButton;
        AutomationProperties.SetName(DrinkButton, text.DrinkConfirmedAutomationName);
        NextTimeText.Text = nextReminderAt is null
            ? text.ConfirmAfterMinutes(settings.IntervalMinutes)
            : text.ExpectedNextReminder(nextReminderAt.Value);
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
