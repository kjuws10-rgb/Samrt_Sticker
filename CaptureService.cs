using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows;
using System.IO;
using Forms = System.Windows.Forms;

namespace SmartSticker;

public static class CaptureService
{
    public static string Capture(NoteStore store, CaptureMode mode = CaptureMode.FullScreen)
    {
        var bounds = Forms.SystemInformation.VirtualScreen;
        CaptureRegion? region = null;
        if (mode != CaptureMode.FullScreen)
        {
            var selector = new CaptureSelector(mode == CaptureMode.Freeform);
            if (selector.ShowDialog() != true || selector.Region is null) throw new OperationCanceledException("캡처가 취소되었습니다.");
            region = selector.Region; bounds = new Rectangle((int)region.ScreenRect.X, (int)region.ScreenRect.Y, (int)region.ScreenRect.Width, (int)region.ScreenRect.Height);
        }
        using var bitmap = new Bitmap(bounds.Width, bounds.Height);
        using (var graphics = Graphics.FromImage(bitmap)) graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
        if (region?.Polygon is { Count: > 2 } polygon) ApplyFreeformMask(bitmap, polygon, region.ScreenRect);
        Directory.CreateDirectory(store.ImageDirectory);
        var path = Path.Combine(store.ImageDirectory, $"capture-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png"); bitmap.Save(path, ImageFormat.Png); return path;
    }
    private static void ApplyFreeformMask(Bitmap source, List<System.Windows.Point> points, Rect rect)
    {
        using var masked = new Bitmap(source.Width, source.Height); using var graphics = Graphics.FromImage(masked); graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = new GraphicsPath(); path.AddPolygon(points.Select(point => new PointF((float)(point.X - rect.X), (float)(point.Y - rect.Y))).ToArray()); graphics.SetClip(path); graphics.DrawImageUnscaled(source, 0, 0);
        using var target = Graphics.FromImage(source); target.Clear(Color.Transparent); target.DrawImageUnscaled(masked, 0, 0);
    }
}
