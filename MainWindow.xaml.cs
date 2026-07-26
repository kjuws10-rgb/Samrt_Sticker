using System.Windows;
using System.Windows.Media.Imaging;

namespace SmartSticker;

public partial class MainWindow : Window
{
    private readonly NoteStore _store;
    private readonly SettingsStore _settings;
    private bool _allowClose;
    public MainWindow(NoteStore store, SettingsStore settings) { InitializeComponent(); _store = store; _settings = settings; Closing += (_, e) => { if (!_allowClose) { e.Cancel = true; Hide(); } }; }
    private void NewNote_Click(object sender, RoutedEventArgs e) => CreateBlankNote();
    private void Capture_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Hide briefly so the preview does not capture the launcher itself.
            Hide(); System.Threading.Thread.Sleep(180);
            var path = CaptureService.Capture(_store);
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
    public void CreateBlankNote() => CreateNote(null);
    public void CaptureNewNote() => Capture_Click(this, new RoutedEventArgs());
    public void ShowDashboard() { Show(); WindowState = WindowState.Normal; Activate(); }
    public void PrepareForExit() => _allowClose = true;
    private void CreateNote(string? imagePath)
    {
        var note = new NoteModel { ImagePath = imagePath, Text = imagePath is null ? "새 메모" : "화면 캡처", IsPinned = _settings.Current.DefaultPinned };
        note.FontFamily = _settings.Current.DefaultFontFamily; note.FontSize = _settings.Current.DefaultFontSize;
        _store.Add(note); new NoteWindow(_store, note, _settings).Show();
    }
    private void Settings_Click(object sender, RoutedEventArgs e) => new SettingsWindow(_settings) { Owner = this }.ShowDialog();
}
