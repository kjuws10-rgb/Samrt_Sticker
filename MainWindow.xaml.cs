using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;

namespace SmartSticker;

public partial class MainWindow : Window
{
    private readonly NoteStore _store;
    private readonly SettingsStore _settings;
    public MainWindow(NoteStore store, SettingsStore settings) { InitializeComponent(); _store = store; _settings = settings; }
    private void NewNote_Click(object sender, RoutedEventArgs e) => CreateNote(null);
    private void Capture_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Hide briefly so the preview does not capture the launcher itself.
            Hide(); System.Threading.Thread.Sleep(180);
            var bounds = Forms.SystemInformation.VirtualScreen;
            using var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var graphics = Graphics.FromImage(bitmap)) graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
            Directory.CreateDirectory(_store.ImageDirectory);
            var path = Path.Combine(_store.ImageDirectory, $"capture-{DateTime.Now:yyyyMMdd-HHmmss}.png");
            bitmap.Save(path, ImageFormat.Png);
            if (_settings.Current.CopyCaptureToClipboard)
            {
                var image = new BitmapImage(); image.BeginInit(); image.UriSource = new Uri(path); image.CacheOption = BitmapCacheOption.OnLoad; image.EndInit();
                System.Windows.Clipboard.SetImage(image);
            }
            Show(); Activate();
            CreateNote(path);
        }
        catch (Exception ex) { Show(); System.Windows.MessageBox.Show($"화면 캡처에 실패했습니다.\n{ex.Message}", "Smart Sticker"); }
    }
    private void CreateNote(string? imagePath)
    {
        var note = new NoteModel { ImagePath = imagePath, Text = imagePath is null ? "새 메모" : "화면 캡처", IsPinned = _settings.Current.DefaultPinned };
        _store.Add(note); new NoteWindow(_store, note).Show();
    }
    private void Settings_Click(object sender, RoutedEventArgs e) => new SettingsWindow(_settings) { Owner = this }.ShowDialog();
}
