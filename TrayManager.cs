using System.Drawing;
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
        _icon = new Forms.NotifyIcon { Icon = StickerIcon.Create(), Text = "Smart Sticker", Visible = true, ContextMenuStrip = menu };
        _icon.DoubleClick += (_, _) => showDashboard();
    }
    public void Dispose() => _icon.Dispose();
}
