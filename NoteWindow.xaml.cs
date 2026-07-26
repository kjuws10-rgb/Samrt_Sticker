using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace SmartSticker;

public partial class NoteWindow : Window
{
    private readonly NoteStore _store;
    private readonly NoteModel _note;
    private readonly string[] _colors = ["#FFF6B7", "#FFCFF4D2", "#FFCFE8FF", "#FFF7D1E2", "#FFE6D9FF"];
    private bool _ready;

    public NoteWindow(NoteStore store, NoteModel note)
    {
        InitializeComponent(); _store = store; _note = note;
        Left = note.Left; Top = note.Top; Width = note.Width; Height = note.Height;
        Editor.Text = note.Text; Topmost = note.IsPinned; PinButton.Content = Topmost ? "📌 고정됨" : "📌 고정";
        SetColor(note.Color); LoadImage(); _ready = true;
        LocationChanged += (_, _) => Save(); SizeChanged += (_, _) => Save(); Closing += (_, _) => Save();
    }
    private void LoadImage()
    {
        if (string.IsNullOrWhiteSpace(_note.ImagePath) || !File.Exists(_note.ImagePath)) return;
        Preview.Source = new BitmapImage(new Uri(_note.ImagePath)); Preview.Visibility = Visibility.Visible;
    }
    private void Editor_TextChanged(object sender, TextChangedEventArgs e) => Save();
    private void Color_Click(object sender, RoutedEventArgs e)
    {
        var index = Array.IndexOf(_colors, _note.Color); SetColor(_colors[(index + 1) % _colors.Length]); Save();
    }
    private void Pin_Click(object sender, RoutedEventArgs e)
    {
        Topmost = !Topmost; PinButton.Content = Topmost ? "📌 고정됨" : "📌 고정"; Save();
    }
    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (System.Windows.MessageBox.Show("이 메모를 삭제할까요?", "Smart Sticker", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        _store.Remove(_note); _ready = false; Close();
    }
    private void SetColor(string color) { _note.Color = color; Root.Background = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString(color)!; }
    private void Save()
    {
        if (!_ready) return;
        _note.Text = Editor.Text; _note.IsPinned = Topmost; _note.Left = Left; _note.Top = Top; _note.Width = Width; _note.Height = Height; _store.Save();
    }
}
