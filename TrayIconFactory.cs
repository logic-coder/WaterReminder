using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace WaterReminder;

public static class TrayIconFactory
{
    public static Icon Create()
    {
        using var bitmap = new Bitmap(64, 64);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        using var outerBrush = new LinearGradientBrush(
            new Rectangle(6, 6, 52, 52),
            Color.FromArgb(255, 81, 163, 255),
            Color.FromArgb(255, 0, 120, 212),
            60f);
        using var shadowBrush = new SolidBrush(Color.FromArgb(50, 2, 24, 43));
        using var highlightBrush = new SolidBrush(Color.FromArgb(110, 255, 255, 255));

        using var shadowPath = CreateDropletPath();
        using var dropletPath = CreateDropletPath();
        using var highlightPath = new GraphicsPath();

        shadowPath.Transform(new Matrix(1, 0, 0, 1, 2, 3));
        graphics.FillPath(shadowBrush, shadowPath);
        graphics.FillPath(outerBrush, dropletPath);

        highlightPath.AddEllipse(18, 12, 11, 16);
        graphics.FillPath(highlightBrush, highlightPath);

        var iconHandle = bitmap.GetHicon();
        try
        {
            using var temporaryIcon = Icon.FromHandle(iconHandle);
            return (Icon)temporaryIcon.Clone();
        }
        finally
        {
            DestroyIcon(iconHandle);
        }
    }

    private static GraphicsPath CreateDropletPath()
    {
        var path = new GraphicsPath();
        path.StartFigure();
        path.AddBezier(new Point(32, 6), new Point(16, 22), new Point(10, 32), new Point(10, 42));
        path.AddBezier(new Point(10, 42), new Point(10, 54), new Point(20, 58), new Point(32, 58));
        path.AddBezier(new Point(32, 58), new Point(44, 58), new Point(54, 54), new Point(54, 42));
        path.AddBezier(new Point(54, 42), new Point(54, 32), new Point(48, 22), new Point(32, 6));
        path.CloseFigure();
        return path;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
