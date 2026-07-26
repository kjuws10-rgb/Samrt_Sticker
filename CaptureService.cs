using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Forms = System.Windows.Forms;

namespace SmartSticker;

public static class CaptureService
{
    public static string Capture(NoteStore store)
    {
        var bounds = Forms.SystemInformation.VirtualScreen;
        using var bitmap = new Bitmap(bounds.Width, bounds.Height);
        using (var graphics = Graphics.FromImage(bitmap)) graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
        Directory.CreateDirectory(store.ImageDirectory);
        var path = Path.Combine(store.ImageDirectory, $"capture-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
        bitmap.Save(path, ImageFormat.Png);
        return path;
    }
}
