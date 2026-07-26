using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Input;

namespace SmartSticker;

public partial class NoteWindow : Window
{
    private readonly NoteStore _store;
    private readonly NoteModel _note;
    private readonly SettingsStore? _settings;
    private readonly string[] _colors = ["#FFFFF6C9", "#FFDDF5D6", "#FFDCEEFF", "#FFFFDCE7", "#FFE9E0FF"];
    private bool _ready;

    public Guid NoteId => _note.Id;
    public NoteWindow(NoteStore store, NoteModel note, SettingsStore? settings = null)
    {
        InitializeComponent(); _store = store; _note = note; _settings = settings;
        Left = note.Left; Top = note.Top; Width = note.Width; Height = note.Height; Topmost = note.IsPinned;
        SetColor(note.Color); Editor.FontFamily = new System.Windows.Media.FontFamily(note.FontFamily); Editor.FontSize = note.FontSize; LoadDocument(); LoadImage(); _ready = true;
        LocationChanged += (_, _) => Save(); SizeChanged += (_, _) => Save(); Closing += (_, _) => Save();
    }
    private void LoadDocument()
    {
        var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
        if (!string.IsNullOrWhiteSpace(_note.RtfText)) { using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_note.RtfText)); range.Load(stream, System.Windows.DataFormats.Rtf); }
        else range.Text = _note.Text;
    }
    private void LoadImage()
    {
        if (string.IsNullOrWhiteSpace(_note.ImagePath) || !File.Exists(_note.ImagePath)) return;
        var image = new BitmapImage(); image.BeginInit(); image.UriSource = new Uri(_note.ImagePath); image.CacheOption = BitmapCacheOption.OnLoad; image.EndInit();
        Preview.Source = image; ImageCard.Visibility = Visibility.Visible;
    }
    private void NewNote_Click(object sender, RoutedEventArgs e) { var note = new NoteModel { Text = "새 메모", FontFamily = _settings?.Current.DefaultFontFamily ?? "맑은 고딕", FontSize = _settings?.Current.DefaultFontSize ?? 16 }; _store.Add(note); new NoteWindow(_store, note, _settings).Show(); }
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.OriginalSource is System.Windows.Controls.Button) return; try { DragMove(); } catch { } }
    private void More_Click(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu();
        var color = new MenuItem { Header = "색상 선택" };
        foreach (var shade in _colors) { var swatch = new MenuItem { Header = "     ", Background = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString(shade)! }; swatch.Click += (_, _) => { SetColor(shade); Save(); }; color.Items.Add(swatch); }
        var delete = new MenuItem { Header = "메모 영구 삭제" }; delete.Click += (_, _) => DeleteNote(); menu.Items.Add(color); menu.Items.Add(new Separator()); menu.Items.Add(delete); menu.IsOpen = true;
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
    private void DeleteNote()
    {
        if (System.Windows.MessageBox.Show("이 메모를 영구 삭제할까요?", "Smart Sticker", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        _store.Remove(_note); _ready = false; Close();
    }
    private void SaveImage_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_note.ImagePath) || !File.Exists(_note.ImagePath)) return;
        var dialog = new Microsoft.Win32.SaveFileDialog { Title = "캡처 이미지 저장", FileName = Path.GetFileName(_note.ImagePath), Filter = "PNG 이미지|*.png|모든 파일|*.*" };
        if (dialog.ShowDialog() == true) File.Copy(_note.ImagePath, dialog.FileName, true);
    }
    private void Capture_Click(object sender, RoutedEventArgs e)
    {
        try { Hide(); System.Threading.Thread.Sleep(180); _note.ImagePath = CaptureService.Capture(_store, _settings?.Current.CaptureMode ?? CaptureMode.FullScreen); Show(); Activate(); LoadImage(); Save(); }
        catch (Exception ex) { Show(); System.Windows.MessageBox.Show($"캡처에 실패했습니다.\n{ex.Message}", "Smart Sticker"); }
    }
    private void Bold_Click(object sender, RoutedEventArgs e) => Editor.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, Editor.Selection.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold) ? FontWeights.Normal : FontWeights.Bold);
    private void Italic_Click(object sender, RoutedEventArgs e) => Editor.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, Editor.Selection.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic) ? FontStyles.Normal : FontStyles.Italic);
    private void Underline_Click(object sender, RoutedEventArgs e) => Editor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, Editor.Selection.GetPropertyValue(Inline.TextDecorationsProperty) == TextDecorations.Underline ? null : TextDecorations.Underline);
    private void Strike_Click(object sender, RoutedEventArgs e) => Editor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, Editor.Selection.GetPropertyValue(Inline.TextDecorationsProperty) == TextDecorations.Strikethrough ? null : TextDecorations.Strikethrough);
    private void Bullets_Click(object sender, RoutedEventArgs e) => EditingCommands.ToggleBullets.Execute(null, Editor);
    private void Editor_TextChanged(object sender, TextChangedEventArgs e) => Save();
    private void SetColor(string color) { _note.Color = color; Root.Background = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString(color)!; }
    private void Save()
    {
        if (!_ready) return;
        var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd); using var stream = new MemoryStream(); range.Save(stream, System.Windows.DataFormats.Rtf);
        _note.RtfText = System.Text.Encoding.UTF8.GetString(stream.ToArray()); _note.Text = range.Text.TrimEnd('\r', '\n'); _note.IsPinned = Topmost; _note.Left = Left; _note.Top = Top; _note.Width = Width; _note.Height = Height; _note.UpdatedAt = DateTime.Now; _store.Save();
    }
}
