using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

namespace SmartSticker;

public partial class MainWindow : Window
{
    private readonly NoteStore _store;
    private readonly SettingsStore _settings;
    private readonly ObservableCollection<NoteListItem> _items = [];
    private bool _allowClose;
    public MainWindow(NoteStore store, SettingsStore settings)
    {
        InitializeComponent(); _store = store; _settings = settings; NotesList.ItemsSource = _items;
        using var icon = StickerIcon.Create(); Icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        Closing += (_, e) => { if (!_allowClose) { e.Cancel = true; Hide(); } };
    }
    public void RefreshNotes()
    {
        if (!Dispatcher.CheckAccess()) { Dispatcher.Invoke(RefreshNotes); return; }
        var query = SearchBox?.Text?.Trim() ?? "";
        var notes = _store.Notes.OrderByDescending(note => note.UpdatedAt).Where(note => string.IsNullOrWhiteSpace(query) || note.Text.Contains(query, StringComparison.OrdinalIgnoreCase));
        _items.Clear(); foreach (var note in notes) _items.Add(new NoteListItem(note));
    }
    private void NewNote_Click(object sender, RoutedEventArgs e) => CreateBlankNote();
    private void Hide_Click(object sender, RoutedEventArgs e) => Hide();
    private void Search_TextChanged(object sender, TextChangedEventArgs e) => RefreshNotes();
    private void NotesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NotesList.SelectedItem is not NoteListItem item) return;
        var open = System.Windows.Application.Current.Windows.OfType<NoteWindow>().FirstOrDefault(window => window.NoteId == item.Note.Id);
        if (open is null) new NoteWindow(_store, item.Note, _settings).Show(); else { open.Show(); open.Activate(); }
        NotesList.SelectedItem = null;
    }
    private void Capture_Click(object sender, RoutedEventArgs e) => CaptureNewNote();
    public void CaptureNewNote()
    {
        try
        {
            Hide(); System.Threading.Thread.Sleep(180); var path = CaptureService.Capture(_store, _settings.Current.CaptureMode);
            if (_settings.Current.CopyCaptureToClipboard) { var image = new BitmapImage(); image.BeginInit(); image.UriSource = new Uri(path); image.CacheOption = BitmapCacheOption.OnLoad; image.EndInit(); System.Windows.Clipboard.SetImage(image); }
            ShowDashboard(); CreateNote(path);
        }
        catch (Exception ex) { ShowDashboard(); System.Windows.MessageBox.Show($"화면 캡처에 실패했습니다.\n{ex.Message}", "Smart Sticker"); }
    }
    public void CreateBlankNote() => CreateNote(null);
    public void ShowDashboard() { Show(); WindowState = WindowState.Normal; Activate(); RefreshNotes(); }
    public void PrepareForExit() => _allowClose = true;
    private void CreateNote(string? imagePath)
    {
        var note = new NoteModel { ImagePath = imagePath, Text = imagePath is null ? "새 메모" : "화면 캡처", IsPinned = _settings.Current.DefaultPinned, FontFamily = _settings.Current.DefaultFontFamily, FontSize = _settings.Current.DefaultFontSize };
        _store.Add(note); new NoteWindow(_store, note, _settings).Show();
    }
    private void Settings_Click(object sender, RoutedEventArgs e) => new SettingsWindow(_settings) { Owner = this }.ShowDialog();
}

public sealed class NoteListItem
{
    public NoteListItem(NoteModel note) { Note = note; }
    public NoteModel Note { get; }
    public string Preview => string.IsNullOrWhiteSpace(Note.Text) ? "메모를 작성하세요..." : Note.Text.Trim();
    public string TimeLabel => Note.UpdatedAt.Date == DateTime.Today ? Note.UpdatedAt.ToString("오전 h:mm").Replace("오전 0", "오전 12") : Note.UpdatedAt.ToString("M월 d일");
}
