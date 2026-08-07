using System.Runtime.InteropServices;

namespace SmartSticker;

public static class DpiAwareness
{
    public static void EnablePerMonitorV2()
    {
        try { SetProcessDpiAwarenessContext(new IntPtr(-4)); } catch { }
    }
    [DllImport("user32.dll")] private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
}
