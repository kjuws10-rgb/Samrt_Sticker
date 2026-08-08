using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace SmartSticker;

public sealed class HotkeyManager : IDisposable
{
    private const int WmHotkey = 0x0312;
    private readonly int _id;
    private bool _registered;
    public event Action? Pressed;
    public HotkeyManager(int id)
    {
        _id = id;
        ComponentDispatcher.ThreadPreprocessMessage += OnMessage;
    }
    public bool Register(string shortcut)
    {
        Unregister();
        if (!TryParse(shortcut, out var modifiers, out var key)) return false;
        _registered = RegisterHotKey(IntPtr.Zero, _id, modifiers, (uint)KeyInterop.VirtualKeyFromKey(key));
        return _registered;
    }
    public static bool AreEquivalent(string? first, string? second)
    {
        return TryParse(first, out var firstModifiers, out var firstKey)
            && TryParse(second, out var secondModifiers, out var secondKey)
            && firstModifiers == secondModifiers
            && firstKey == secondKey;
    }
    public static bool TryParse(string? shortcut, out uint modifiers, out Key key)
    {
        modifiers = 0; key = Key.None;
        var parts = (shortcut ?? "").Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return false;
        foreach (var part in parts[..^1])
        {
            uint flag;
            if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) || part.Equals("Control", StringComparison.OrdinalIgnoreCase)) flag = 2;
            else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase)) flag = 1;
            else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase)) flag = 4;
            else if (part.Equals("Win", StringComparison.OrdinalIgnoreCase) || part.Equals("Windows", StringComparison.OrdinalIgnoreCase)) flag = 8;
            else return false;
            if ((modifiers & flag) != 0) return false;
            modifiers |= flag;
        }
        return modifiers != 0 && Enum.TryParse(parts[^1], true, out key) && key != Key.None && KeyInterop.VirtualKeyFromKey(key) != 0;
    }
    private void OnMessage(ref MSG msg, ref bool handled) { if (msg.message == WmHotkey && msg.wParam.ToInt32() == _id) { Pressed?.Invoke(); handled = true; } }
    private void Unregister() { if (_registered) { UnregisterHotKey(IntPtr.Zero, _id); _registered = false; } }
    public void ClearRegistration() => Unregister();
    public void Dispose() { Unregister(); ComponentDispatcher.ThreadPreprocessMessage -= OnMessage; }
    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint virtualKey);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
