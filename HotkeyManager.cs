using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace SmartSticker;

public sealed class HotkeyManager : IDisposable
{
    private const int Id = 45017;
    private const int WmHotkey = 0x0312;
    private bool _registered;
    public event Action? Pressed;
    public HotkeyManager() => ComponentDispatcher.ThreadPreprocessMessage += OnMessage;
    public bool Register(string shortcut)
    {
        Unregister();
        if (!TryParse(shortcut, out var modifiers, out var key)) return false;
        _registered = RegisterHotKey(IntPtr.Zero, Id, modifiers, (uint)KeyInterop.VirtualKeyFromKey(key));
        return _registered;
    }
    public static bool TryParse(string? shortcut, out uint modifiers, out Key key)
    {
        modifiers = 0; key = Key.None;
        var parts = (shortcut ?? "").Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return false;
        foreach (var part in parts[..^1])
        {
            if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) || part.Equals("Control", StringComparison.OrdinalIgnoreCase)) modifiers |= 2;
            else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase)) modifiers |= 1;
            else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase)) modifiers |= 4;
            else if (part.Equals("Win", StringComparison.OrdinalIgnoreCase) || part.Equals("Windows", StringComparison.OrdinalIgnoreCase)) modifiers |= 8;
            else return false;
        }
        return modifiers != 0 && Enum.TryParse(parts[^1], true, out key) && key != Key.None && KeyInterop.VirtualKeyFromKey(key) != 0;
    }
    private void OnMessage(ref MSG msg, ref bool handled) { if (msg.message == WmHotkey && msg.wParam.ToInt32() == Id) { Pressed?.Invoke(); handled = true; } }
    private void Unregister() { if (_registered) { UnregisterHotKey(IntPtr.Zero, Id); _registered = false; } }
    public void Dispose() { Unregister(); ComponentDispatcher.ThreadPreprocessMessage -= OnMessage; }
    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint virtualKey);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
