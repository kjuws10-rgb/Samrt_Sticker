using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace SmartSticker;

public static class SafeClipboard
{
    private static BitmapSource? _internalImage;
    private static string? _internalText;

    public static bool SetImage(BitmapSource image)
    {
        var copy = BitmapFrame.Create(image); copy.Freeze(); _internalImage = copy;
        return Retry(() => System.Windows.Clipboard.SetImage(copy));
    }
    public static BitmapSource? GetImage()
    {
        BitmapSource? result = null;
        if (Retry(() => { if (System.Windows.Clipboard.ContainsImage()) result = System.Windows.Clipboard.GetImage(); }) && result is not null) return result;
        return _internalImage;
    }
    public static bool SetText(string text) { _internalText = text; return Retry(() => System.Windows.Clipboard.SetText(text)); }
    public static string? GetText()
    {
        string? result = null;
        if (Retry(() => { if (System.Windows.Clipboard.ContainsText()) result = System.Windows.Clipboard.GetText(); }) && result is not null) return result;
        return _internalText;
    }
    private static bool Retry(Action action)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try { action(); return true; }
            catch (ExternalException) { if (attempt < 2) Thread.Sleep(35); }
            catch (InvalidOperationException) { if (attempt < 2) Thread.Sleep(35); }
        }
        return false;
    }
}
