using System.Runtime.InteropServices;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SmartSticker;

public static class SafeClipboard
{
    private static BitmapSource? _internalImage;
    private static string? _internalText;

    public static bool SetImage(BitmapSource image)
    {
        var copy = BitmapFrame.Create(image); copy.Freeze();
        _internalImage = copy;
        _internalText = null;

        return Retry(() =>
        {
            var data = new System.Windows.DataObject();
            data.SetImage(copy);

            // Some remote/virtualized applications only advertise PNG rather than
            // the WPF bitmap formats. Publish both without changing image quality.
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(copy));
            var png = new MemoryStream();
            encoder.Save(png);
            png.Position = 0;
            data.SetData("PNG", png, false);

            System.Windows.Clipboard.SetDataObject(data, true);
        });
    }
    public static BitmapSource? GetImage()
    {
        BitmapSource? result = null;
        var clipboardWasRead = Retry(() =>
        {
            if (System.Windows.Clipboard.ContainsImage())
                result = System.Windows.Clipboard.GetImage();
        });

        // An accessible clipboard that contains no image must not fall back to an
        // old internal image. The caller can then correctly try text formats.
        if (clipboardWasRead) return result;
        return _internalImage;
    }
    public static bool SetText(string text)
    {
        _internalText = text;
        _internalImage = null;
        return Retry(() =>
        {
            var data = new System.Windows.DataObject();
            data.SetData(System.Windows.DataFormats.UnicodeText, text, true);
            data.SetData(System.Windows.DataFormats.Text, text, true);
            data.SetData(System.Windows.DataFormats.StringFormat, text, true);
            System.Windows.Clipboard.SetDataObject(data, true);
        });
    }
    public static string? GetText()
    {
        string? result = null;
        var clipboardWasRead = Retry(() =>
        {
            if (System.Windows.Clipboard.ContainsText(System.Windows.TextDataFormat.UnicodeText))
                result = System.Windows.Clipboard.GetText(System.Windows.TextDataFormat.UnicodeText);
            else if (System.Windows.Clipboard.ContainsText(System.Windows.TextDataFormat.Text))
                result = System.Windows.Clipboard.GetText(System.Windows.TextDataFormat.Text);
        });
        if (clipboardWasRead) return result;
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
