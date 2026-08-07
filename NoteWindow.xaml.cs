using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;

namespace SmartSticker;

public partial class NoteWindow : Window
{
    private readonly NoteStore _store;
    private readonly NoteModel _note;
    private readonly SettingsStore? _settings;
    private readonly string[] _colors = ["#FFFFF6C9", "#FFFFD7A8", "#FFFFC9D6", "#FFE9D6FF", "#FFD8E8FF", "#FFCDEFE5", "#FFD9F0C2", "#FFE4E7EB", "#FFD1D5DB", "#FFC8C8C8"];
    private bool _ready;
    private bool _isCapturing;
    private bool _isPanning;
    private System.Windows.Point _panStart;
    private double _panHorizontal;
    private double _panVertical;
    private readonly DispatcherTimer _reminderTimer = new() { Interval = TimeSpan.FromSeconds(15) };

    public Guid NoteId => _note.Id;
    public NoteWindow(NoteStore store, NoteModel note, SettingsStore? settings = null)
    {
        InitializeComponent(); _store = store; _note = note; _settings = settings;
        _reminderTimer.Tick += (_, _) => CheckReminder(); _reminderTimer.Start();
        var inlineItem = new System.Windows.Controls.MenuItem { Header = "본문 커서 위치에 이미지 삽입" }; inlineItem.Click += (_, _) => InsertImageIntoText(); Preview.ContextMenu.Items.Insert(1, inlineItem);
        Left = note.Left; Top = note.Top; Width = note.Width; Height = note.Height; Topmost = note.IsPinned;
        SetColor(note.Color); Opacity = 1 - (note.Transparency / 100); Editor.FontFamily = new System.Windows.Media.FontFamily(note.FontFamily); Editor.FontSize = note.FontSize; LoadDocument(); LoadImage();
        ImageScale.ScaleX = note.ImageScale; ImageScale.ScaleY = note.ImageScale; ImageTranslate.X = note.ImageOffsetX; ImageTranslate.Y = note.ImageOffsetY; _ready = true;
        LocationChanged += (_, _) => Save(); SizeChanged += (_, _) => Save(); Closing += (_, _) => Save();
    }
    private void LoadDocument()
    {
        var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
        if (!string.IsNullOrWhiteSpace(_note.XamlDocument)) { using var stream = new MemoryStream(Convert.FromBase64String(_note.XamlDocument)); range.Load(stream, System.Windows.DataFormats.XamlPackage); }
        else if (!string.IsNullOrWhiteSpace(_note.RtfText)) { using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_note.RtfText)); range.Load(stream, System.Windows.DataFormats.Rtf); }
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
        var font = new MenuItem { Header = "글꼴" };
        foreach (var name in new[] { "맑은 고딕", "Segoe UI", "Arial", "나눔고딕", "Consolas" }) { var item = new MenuItem { Header = name, FontFamily = new System.Windows.Media.FontFamily(name) }; item.Click += (_, _) => { _note.FontFamily = name; Editor.FontFamily = new System.Windows.Media.FontFamily(name); Save(); }; font.Items.Add(item); }
        var reminder = new MenuItem { Header = "이벤트 알림: 1시간 후" }; reminder.Click += (_, _) => { _note.ReminderAt = DateTime.Now.AddHours(1); _note.ReminderMinutesBefore = 0; _note.ReminderNotified = false; Save(); };
        var delete = new MenuItem { Header = "메모 영구 삭제" }; delete.Click += (_, _) => DeleteNote(); menu.Items.Add(color); menu.Items.Add(font); menu.Items.Add(reminder); menu.Items.Add(new Separator()); menu.Items.Add(delete); menu.IsOpen = true;
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
    private void Opacity_Click(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu(); var panel = new StackPanel { Margin = new Thickness(10), Width = 160 };
        panel.Children.Add(new TextBlock { Text = "메모 투명도", FontWeight = FontWeights.SemiBold });
        var label = new TextBlock { Text = $"{_note.Transparency:0}%", Margin = new Thickness(0, 5, 0, 0), HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
        var slider = new Slider { Minimum = 0, Maximum = 95, Value = _note.Transparency };
        slider.ValueChanged += (_, _) => { Opacity = 1 - (slider.Value / 100); label.Text = $"{slider.Value:0}%"; _note.Transparency = slider.Value; Save(); };
        panel.Children.Add(slider); panel.Children.Add(label); menu.Items.Add(new MenuItem { Header = panel, StaysOpenOnClick = true }); menu.IsOpen = true;
    }
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
    private void CopyImage_Click(object sender, RoutedEventArgs e)
    {
        if (Preview.Source is BitmapSource image) System.Windows.Clipboard.SetImage(image);
    }
    private void DeleteImage_Click(object sender, RoutedEventArgs e)
    {
        if (System.Windows.MessageBox.Show("이 메모에서 첨부 이미지를 삭제할까요?", "Smart Sticker", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        _note.ImagePath = null; Preview.Source = null; ImageCard.Visibility = Visibility.Collapsed; Save();
    }
    private void InsertImageIntoText()
    {
        if (string.IsNullOrWhiteSpace(_note.ImagePath) || !File.Exists(_note.ImagePath)) return;
        var image = new System.Windows.Controls.Image { MaxWidth = 240, MaxHeight = 180, Stretch = Stretch.Uniform, Margin = new Thickness(4) };
        var source = new BitmapImage(); source.BeginInit(); source.UriSource = new Uri(_note.ImagePath); source.CacheOption = BitmapCacheOption.OnLoad; source.EndInit(); image.Source = source;
        var paragraph = Editor.CaretPosition.Paragraph ?? Editor.Document.Blocks.FirstBlock as Paragraph;
        paragraph?.Inlines.Add(new InlineUIContainer(image));
        ImageCard.Visibility = Visibility.Collapsed; Save(); Editor.Focus();
    }
    private void Editor_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.V || Keyboard.Modifiers != ModifierKeys.Control || !System.Windows.Clipboard.ContainsImage()) return;
        var image = System.Windows.Clipboard.GetImage();
        if (image is null) return;
        Directory.CreateDirectory(_store.ImageDirectory);
        var path = Path.Combine(_store.ImageDirectory, $"pasted-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(image));
        using (var stream = File.Create(path)) encoder.Save(stream);
        _note.ImagePath = path; LoadImage(); Save(); e.Handled = true;
    }
    private void Preview_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control) return;
        var next = Math.Clamp(ImageScale.ScaleX + (e.Delta > 0 ? .12 : -.12), .25, 4.0);
        ImageScale.ScaleX = next; ImageScale.ScaleY = next; SaveImageView(); e.Handled = true;
    }
    private void Preview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) { ImageScale.ScaleX = 1; ImageScale.ScaleY = 1; ImageTranslate.X = 0; ImageTranslate.Y = 0; SaveImageView(); e.Handled = true; return; }
        if (ImageScale.ScaleX <= 1) return;
        _isPanning = true; _panStart = e.GetPosition(ImageScroll); _panHorizontal = ImageTranslate.X; _panVertical = ImageTranslate.Y; Preview.CaptureMouse(); Preview.Cursor = System.Windows.Input.Cursors.Hand; e.Handled = true;
    }
    private void Preview_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_isPanning) return;
        var current = e.GetPosition(ImageScroll); ImageTranslate.X = _panHorizontal + (current.X - _panStart.X); ImageTranslate.Y = _panVertical + (current.Y - _panStart.Y);
    }
    private void Preview_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { if (!_isPanning) return; _isPanning = false; Preview.ReleaseMouseCapture(); Preview.Cursor = System.Windows.Input.Cursors.Arrow; SaveImageView(); }
    private void SaveImageView() { if (!_ready) return; _note.ImageScale = ImageScale.ScaleX; _note.ImageOffsetX = ImageTranslate.X; _note.ImageOffsetY = ImageTranslate.Y; Save(); }
    private void Capture_Click(object sender, RoutedEventArgs e)
    {
        if (_isCapturing) return;
        _isCapturing = true;
        try { Hide(); System.Threading.Thread.Sleep(180); _note.ImagePath = CaptureService.Capture(_store, _settings?.Current.CaptureMode ?? CaptureMode.FullScreen); Show(); Activate(); LoadImage(); Save(); }
        catch (OperationCanceledException) { Show(); }
        catch (Exception ex) { Show(); System.Windows.MessageBox.Show($"캡처에 실패했습니다.\n{ex.Message}", "Smart Sticker"); }
        finally { _isCapturing = false; }
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
        _note.RtfText = System.Text.Encoding.UTF8.GetString(stream.ToArray()); using var xamlStream = new MemoryStream(); range.Save(xamlStream, System.Windows.DataFormats.XamlPackage); _note.XamlDocument = Convert.ToBase64String(xamlStream.ToArray()); _note.Text = range.Text.TrimEnd('\r', '\n'); _note.IsPinned = Topmost; _note.Left = Left; _note.Top = Top; _note.Width = Width; _note.Height = Height; _note.NoteOpacity = Opacity; _note.UpdatedAt = DateTime.Now; _store.Save();
    }
    private void CheckReminder()
    {
        if (_note.ReminderAt is not DateTime at || _note.ReminderNotified || DateTime.Now < at.AddMinutes(-_note.ReminderMinutesBefore)) return;
        _note.ReminderNotified = true; Save(); Show(); Activate(); var original = Opacity; var count = 0; var flash = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) }; flash.Tick += (_, _) => { Opacity = Opacity < original ? original : Math.Max(.25, original - .35); if (++count >= 8) { flash.Stop(); Opacity = original; } }; flash.Start(); System.Windows.MessageBox.Show($"이벤트 알림: {at:HH:mm}", "Smart Sticker");
    }
}
