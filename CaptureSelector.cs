using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Forms = System.Windows.Forms;
using WpfPoint = System.Windows.Point;
using WpfRectangle = System.Windows.Shapes.Rectangle;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace SmartSticker;

public sealed class CaptureRegion(Rect screenRect, List<WpfPoint>? polygon)
{
    public Rect ScreenRect { get; } = screenRect;
    public List<WpfPoint>? Polygon { get; } = polygon;
}

public sealed class CaptureSelector : Window
{
    private readonly Canvas _canvas = new();
    private readonly bool _freeform;
    private readonly List<WpfPoint> _points = [];
    private readonly WpfRectangle _rectangle = new() { Stroke = System.Windows.Media.Brushes.White, Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 255, 255, 255)), StrokeThickness = 2, StrokeDashArray = [4, 2] };
    private WpfPoint _start;
    public CaptureRegion? Region { get; private set; }
    public CaptureSelector(bool freeform)
    {
        _freeform = freeform; var bounds = Forms.SystemInformation.VirtualScreen;
        Left = bounds.Left; Top = bounds.Top; Width = bounds.Width; Height = bounds.Height; WindowStyle = WindowStyle.None; AllowsTransparency = true; Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 0, 0, 0)); Topmost = true; Cursor = System.Windows.Input.Cursors.Cross;
        _canvas.Children.Add(_rectangle); Content = _canvas; _canvas.MouseLeftButtonDown += Down; _canvas.MouseMove += Move; _canvas.MouseLeftButtonUp += Up; KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
    }
    private void Down(object sender, MouseButtonEventArgs e) { _start = e.GetPosition(_canvas); _points.Clear(); _points.Add(_start); Mouse.Capture(_canvas); e.Handled = true; }
    private void Move(object sender, WpfMouseEventArgs e)
    {
        if (Mouse.Captured != _canvas) return; var current = e.GetPosition(_canvas); _points.Add(current);
        var x = Math.Min(_start.X, current.X); var y = Math.Min(_start.Y, current.Y); _rectangle.Width = Math.Abs(current.X - _start.X); _rectangle.Height = Math.Abs(current.Y - _start.Y); Canvas.SetLeft(_rectangle, x); Canvas.SetTop(_rectangle, y);
    }
    private void Up(object sender, MouseButtonEventArgs e)
    {
        if (Mouse.Captured != _canvas) return; Mouse.Capture(null); var end = e.GetPosition(_canvas); var x = Math.Min(_start.X, end.X); var y = Math.Min(_start.Y, end.Y); var width = Math.Abs(end.X - _start.X); var height = Math.Abs(end.Y - _start.Y);
        if (width < 4 || height < 4) { Close(); return; }
        Region = new CaptureRegion(new Rect(Left + x, Top + y, width, height), _freeform ? _points.Select(point => new WpfPoint(Left + point.X, Top + point.Y)).ToList() : null); DialogResult = true;
    }
}
