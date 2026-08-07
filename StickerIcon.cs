using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SmartSticker;

public static class StickerIcon
{
    public static Icon Create()
    {
        using var bitmap = new Bitmap(32, 32); using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias; graphics.Clear(Color.Transparent);
        using var shadow = new SolidBrush(Color.FromArgb(45, 0, 0, 0)); graphics.FillRoundedRectangle(shadow, 4, 5, 25, 25, 5);
        using var note = new GraphicsPath(); note.AddRoundedRectangle(new RectangleF(3, 3, 26, 26), 5);
        using var fill = new LinearGradientBrush(new System.Drawing.Point(3, 3), new System.Drawing.Point(28, 29), Color.FromArgb(255, 255, 237, 139), Color.FromArgb(255, 250, 207, 72)); graphics.FillPath(fill, note);
        using var fold = new GraphicsPath(); fold.AddPolygon([new PointF(21, 29), new PointF(29, 21), new PointF(29, 29)]); using var foldFill = new SolidBrush(Color.FromArgb(255, 255, 247, 198)); graphics.FillPath(foldFill, fold);
        using var pen = new Pen(Color.FromArgb(210, 72, 73, 78), 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round }; graphics.DrawLine(pen, 9, 11, 23, 11); graphics.DrawLine(pen, 9, 16, 21, 16); graphics.DrawLine(pen, 9, 21, 18, 21);
        var handle = bitmap.GetHicon(); try { using var temporary = Icon.FromHandle(handle); return (Icon)temporary.Clone(); } finally { DestroyIcon(handle); }
    }
    public static BitmapSource CreateImageSource() { using var icon = Create(); var image = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); image.Freeze(); return image; }
    [DllImport("user32.dll")] private static extern bool DestroyIcon(IntPtr handle);
}

internal static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, float x, float y, float width, float height, float radius) { using var path = new GraphicsPath(); path.AddRoundedRectangle(new RectangleF(x, y, width, height), radius); graphics.FillPath(brush, path); }
    public static void AddRoundedRectangle(this GraphicsPath path, RectangleF rect, float radius) { var diameter = radius * 2; path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90); path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90); path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90); path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90); path.CloseFigure(); }
}
