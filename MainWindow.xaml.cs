using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Effects;
using System.IO;

namespace SmartSticker;

public partial class MainWindow : Window
{
    private readonly NoteStore _store;
    private readonly SettingsStore _settings;
    private readonly ObservableCollection<NoteListItem> _items = [];
    private bool _allowClose;
    private bool _isCapturing;
    private UIElement? _dashboardContent;
    public MainWindow(NoteStore store, SettingsStore settings)
    {
        InitializeComponent(); _store = store; _settings = settings; NotesList.ItemsSource = _items;
        _dashboardContent = Root.Child;
        NotesList.SelectionMode = System.Windows.Controls.SelectionMode.Extended;
        var menu = new ContextMenu(); var delete = new MenuItem { Header = "선택한 메모 삭제" }; delete.Click += DeleteSelected_Click; menu.Items.Add(delete); menu.Opened += (_, _) => delete.Header = NotesList.SelectedItems.Count > 1 ? $"선택한 {NotesList.SelectedItems.Count}개 메모 삭제" : "선택한 메모 삭제"; NotesList.ContextMenu = menu;
        NotesList.PreviewMouseRightButtonDown += NotesList_PreviewMouseRightButtonDown;
        NotesList.PreviewKeyDown += NotesList_PreviewKeyDown;
        NotesList.ItemContainerGenerator.StatusChanged += (_, _) => AttachHoverPreviews();
        ApplyTheme(_settings.Current.Theme);
        using var icon = StickerIcon.Create(); Icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        Closing += (_, e) => { if (!_allowClose) { e.Cancel = true; Hide(); } };
        var jumpList = new JumpList(); jumpList.JumpItems.Add(new JumpTask { Title = "새 메모", Description = "새 스티커 메모 만들기", ApplicationPath = Environment.ProcessPath, Arguments = "--new-note" }); jumpList.JumpItems.Add(new JumpTask { Title = "모든 메모 표시", Description = "저장된 메모 열기", ApplicationPath = Environment.ProcessPath, Arguments = "--show-all" }); JumpList.SetJumpList(System.Windows.Application.Current, jumpList);
    }
    public void RefreshNotes()
    {
        if (!Dispatcher.CheckAccess()) { Dispatcher.Invoke(RefreshNotes); return; }
        var query = SearchBox?.Text?.Trim() ?? "";
        var notes = _store.Notes.OrderByDescending(note => note.UpdatedAt).Where(note => string.IsNullOrWhiteSpace(query) || note.Text.Contains(query, StringComparison.OrdinalIgnoreCase));
        _items.Clear(); foreach (var note in notes) _items.Add(new NoteListItem(note));
    }
    private void AttachHoverPreviews()
    {
        if (NotesList.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated) return;
        foreach (var item in _items)
        {
            if (NotesList.ItemContainerGenerator.ContainerFromItem(item) is not ListBoxItem container || container.ToolTip is not null) continue;
            var preview = CreateHoverPreview(item.Note);
            container.ToolTip = preview;
            container.MouseLeave += (_, _) => preview.IsOpen = false;
        }
    }
    private static System.Windows.Controls.ToolTip CreateHoverPreview(NoteModel note)
    {
        var stack = new System.Windows.Controls.StackPanel { Width = 285, Margin = new Thickness(14, 12, 14, 12) };
        stack.Children.Add(new System.Windows.Controls.TextBlock { Text = "메모 미리보기", FontWeight = FontWeights.SemiBold, FontSize = 12, Foreground = System.Windows.Media.Brushes.DimGray, Margin = new Thickness(0, 0, 0, 7) });
        if (!string.IsNullOrWhiteSpace(note.ImagePath) && File.Exists(note.ImagePath))
        {
            var image = new System.Windows.Controls.Image { MaxHeight = 150, Stretch = Stretch.Uniform, Margin = new Thickness(0, 0, 0, 8) };
            var source = new BitmapImage(); source.BeginInit(); source.UriSource = new Uri(note.ImagePath); source.CacheOption = BitmapCacheOption.OnLoad; source.EndInit(); image.Source = source; stack.Children.Add(image);
        }
        stack.Children.Add(new System.Windows.Controls.TextBlock { Text = string.IsNullOrWhiteSpace(note.Text) ? "메모를 작성하세요..." : note.Text, TextWrapping = TextWrapping.Wrap, MaxHeight = 190, Foreground = System.Windows.Media.Brushes.Black, FontSize = note.FontSize });
        var border = new System.Windows.Controls.Border { Background = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString(note.Color)!, Opacity = .93, CornerRadius = new CornerRadius(8), Child = stack, BorderBrush = System.Windows.Media.Brushes.White, BorderThickness = new Thickness(1) };
        var tip = new System.Windows.Controls.ToolTip { Content = border, Opacity = 0, Placement = PlacementMode.Right, HorizontalOffset = 8, VerticalOffset = -8, StaysOpen = true };
        tip.Opened += (_, _) => tip.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });
        return tip;
    }
    private void NewNote_Click(object sender, RoutedEventArgs e) => CreateBlankNote();
    private void Hide_Click(object sender, RoutedEventArgs e) => Hide();
    private void Search_TextChanged(object sender, TextChangedEventArgs e) => RefreshNotes();
    private void NotesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NotesList.SelectedItem is not NoteListItem item) { UpdateSelectionAppearance(); return; }
        if (Mouse.RightButton == MouseButtonState.Pressed || Keyboard.Modifiers != ModifierKeys.None) { UpdateSelectionAppearance(); return; }
        var open = System.Windows.Application.Current.Windows.OfType<NoteWindow>().FirstOrDefault(window => window.NoteId == item.Note.Id);
        if (open is null) new NoteWindow(_store, item.Note, _settings).Show(); else { open.Show(); open.Activate(); }
        NotesList.SelectedItem = null;
        UpdateSelectionAppearance();
    }
    private void UpdateSelectionAppearance()
    {
        foreach (var item in _items)
        {
            if (NotesList.ItemContainerGenerator.ContainerFromItem(item) is not ListBoxItem container) continue;
            var selected = container.IsSelected;
            container.Opacity = selected ? .62 : 1;
            container.Effect = selected ? new DropShadowEffect { Color = System.Windows.Media.Colors.DimGray, ShadowDepth = 0, BlurRadius = 11, Opacity = .72 } : null;
        }
    }
    private void NotesList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var source = e.OriginalSource as DependencyObject;
        while (source is not null && source is not ListBoxItem) source = VisualTreeHelper.GetParent(source);
        if (source is ListBoxItem item && !item.IsSelected) { NotesList.SelectedItems.Clear(); item.IsSelected = true; }
    }
    private void DeleteSelected_Click(object? sender, RoutedEventArgs e)
    {
        var selected = NotesList.SelectedItems.Cast<NoteListItem>().Select(item => item.Note).ToList();
        if (selected.Count == 0) return;
        var text = selected.Count == 1 ? "선택한 메모를 삭제할까요?" : $"선택한 {selected.Count}개의 메모를 삭제할까요?";
        if (System.Windows.MessageBox.Show(text, "Smart Sticker", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        foreach (var note in selected)
        {
            System.Windows.Application.Current.Windows.OfType<NoteWindow>().FirstOrDefault(window => window.NoteId == note.Id)?.Close();
            _store.Remove(note);
        }
        NotesList.SelectedItems.Clear(); RefreshNotes();
    }
    private void ToggleAllNotesVisibility_Click(object sender, RoutedEventArgs e)
    {
        var notes = System.Windows.Application.Current.Windows.OfType<NoteWindow>().ToList();
        if (notes.Any(note => note.IsVisible)) { foreach (var note in notes) note.Hide(); return; }
        foreach (var note in _store.Notes)
        {
            var open = notes.FirstOrDefault(window => window.NoteId == note.Id);
            if (open is null) new NoteWindow(_store, note, _settings).Show(); else { open.Show(); open.Activate(); }
        }
    }
    private void NotesList_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != System.Windows.Input.Key.Delete || NotesList.SelectedItems.Count == 0) return;
        DeleteSelected_Click(sender, e); e.Handled = true;
    }
    private void Capture_Click(object sender, RoutedEventArgs e) => CaptureNewNote();
    public void CaptureNewNote()
    {
        if (_isCapturing) return;
        _isCapturing = true;
        try
        {
            Hide(); System.Threading.Thread.Sleep(180); var path = CaptureService.Capture(_store, _settings.Current.CaptureMode);
            if (_settings.Current.CopyCaptureToClipboard) { var image = new BitmapImage(); image.BeginInit(); image.UriSource = new Uri(path); image.CacheOption = BitmapCacheOption.OnLoad; image.EndInit(); System.Windows.Clipboard.SetImage(image); }
            ShowDashboard(); CreateNote(path);
        }
        catch (OperationCanceledException) { ShowDashboard(); }
        catch (Exception ex) { ShowDashboard(); System.Windows.MessageBox.Show($"화면 캡처에 실패했습니다.\n{ex.Message}", "Smart Sticker"); }
        finally { _isCapturing = false; }
    }
    public void CreateBlankNote() => CreateNote(null);
    public void ShowDashboard() { Show(); WindowState = WindowState.Normal; Activate(); RefreshNotes(); }
    public void PrepareForExit() => _allowClose = true;
    public void ApplyTheme(string theme) { Root.Background = theme.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(38, 40, 45)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(252, 252, 252)); }
    private void CreateNote(string? imagePath)
    {
        var note = new NoteModel { ImagePath = imagePath, Text = imagePath is null ? "새 메모" : "화면 캡처", IsPinned = _settings.Current.DefaultPinned, FontFamily = _settings.Current.DefaultFontFamily, FontSize = _settings.Current.DefaultFontSize };
        _store.Add(note); new NoteWindow(_store, note, _settings).Show();
    }
    private void Settings_Click(object sender, RoutedEventArgs e) => Root.Child = new SettingsPage(_settings, () => Root.Child = _dashboardContent);
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.OriginalSource is System.Windows.Controls.Button) return; try { DragMove(); } catch { } }
}

public sealed class NoteListItem
{
    public NoteListItem(NoteModel note) { Note = note; }
    public NoteModel Note { get; }
    public string Color => Note.Color;
    public string Preview => string.IsNullOrWhiteSpace(Note.Text) ? "메모를 작성하세요..." : Note.Text.Trim();
    public string TimeLabel => Note.UpdatedAt.Date == DateTime.Today ? Note.UpdatedAt.ToString("오전 h:mm").Replace("오전 0", "오전 12") : Note.UpdatedAt.ToString("M월 d일");
}
