using System.Drawing;
using System.Drawing.Drawing2D;
using Forms = System.Windows.Forms;

namespace SmartSticker;

public sealed class TrayManager : IDisposable
{
    private readonly Forms.NotifyIcon _icon;
    public TrayManager(Action showDashboard, Action newNote, Action capture, Action showAllNotes, Action exit)
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Smart Sticker 열기", null, (_, _) => showDashboard());
        menu.Items.Add("새 메모", null, (_, _) => newNote());
        menu.Items.Add("화면 캡처", null, (_, _) => capture());
        menu.Items.Add("저장된 노트 모두 보기", null, (_, _) => showAllNotes());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("종료", null, (_, _) => exit());
        _icon = new Forms.NotifyIcon { Icon = CreateStickerIcon(), Text = "Smart Sticker", Visible = true, ContextMenuStrip = menu };
        _icon.DoubleClick += (_, _) => showDashboard();
    }
    public void Dispose() => _icon.Dispose();
    private static Icon CreateStickerIcon()
    {
        using var bitmap = new Bitmap(32, 32); using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias; graphics.Clear(Color.Transparent);
        using var path = new GraphicsPath(); path.AddArc(3, 3, 26, 26, 7, 346); path.CloseFigure();
        using var fill = new SolidBrush(Color.FromArgb(255, 255, 224, 91)); graphics.FillPath(fill, path);
        using var pen = new Pen(Color.FromArgb(255, 92, 74, 160), 2); graphics.DrawLine(pen, 10, 11, 23, 11); graphics.DrawLine(pen, 10, 16, 20, 16); graphics.DrawLine(pen, 10, 21, 18, 21);
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
