using System.Drawing;
using System.Drawing.Drawing2D;

namespace SmartSticker;

public static class StickerIcon
{
    public static Icon Create()
    {
        using var bitmap = new Bitmap(32, 32); using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias; graphics.Clear(Color.Transparent);
        using var path = new GraphicsPath(); path.AddArc(3, 3, 26, 26, 7, 346); path.CloseFigure();
        using var fill = new SolidBrush(Color.FromArgb(255, 255, 224, 91)); graphics.FillPath(fill, path);
        using var pen = new Pen(Color.FromArgb(255, 92, 74, 160), 2); graphics.DrawLine(pen, 10, 11, 23, 11); graphics.DrawLine(pen, 10, 16, 20, 16); graphics.DrawLine(pen, 10, 21, 18, 21);
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
