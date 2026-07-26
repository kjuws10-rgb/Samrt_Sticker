using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace SmartSticker;

public sealed class SettingsPage : Grid
{
    public SettingsPage(SettingsStore store, Action goBack)
    {
        Margin = new Thickness(12); RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); RowDefinitions.Add(new RowDefinition()); RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var back = new Button { Content = "←  메모 목록", FontSize = 15, FontWeight = FontWeights.SemiBold, Width = 128, Height = 46, Padding = new Thickness(8, 0, 8, 0), Background = Brushes.Transparent, BorderThickness = new Thickness(0), HorizontalAlignment = HorizontalAlignment.Left, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Center, ToolTip = "메모 목록으로 돌아가기" }; System.Windows.Shell.WindowChrome.SetIsHitTestVisibleInChrome(back, true); back.Click += (_, _) => goBack();
        var title = new TextBlock { Text = "설정", FontSize = 20, FontWeight = FontWeights.SemiBold, Margin = new Thickness(138, 10, 0, 8), VerticalAlignment = VerticalAlignment.Center }; var header = new Grid { Height = 48 }; header.Children.Add(back); header.Children.Add(title); Children.Add(header);
        var s = store.Current; var panel = new StackPanel { Margin = new Thickness(2, 18, 2, 0) }; Grid.SetRow(panel, 1); Children.Add(panel);
        var auto = AddCheck(panel, "Windows 로그인 시 자동 실행", s.StartWithWindows); var restore = AddCheck(panel, "실행 시 이전 메모 복원", s.RestoreNotesOnLaunch); var pin = AddCheck(panel, "새 메모 항상 위에 표시", s.DefaultPinned); var copy = AddCheck(panel, "캡처 이미지를 클립보드에 복사", s.CopyCaptureToClipboard);
        panel.Children.Add(new TextBlock { Text = "화면 캡처 단축키", Margin = new Thickness(0, 18, 0, 3) }); var hotkey = new TextBox { Text = s.CaptureShortcut, Padding = new Thickness(8, 5, 8, 5) }; panel.Children.Add(hotkey);
        panel.Children.Add(new TextBlock { Text = "테마", Margin = new Thickness(0, 14, 0, 3) }); var theme = new ComboBox { SelectedValue = s.Theme }; theme.Items.Add("Light"); theme.Items.Add("Dark"); theme.SelectedItem = s.Theme; panel.Children.Add(theme);
        var save = new Button { Content = "저장", Background = new SolidColorBrush(Color.FromRgb(76, 139, 245)), Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
        save.Click += (_, _) => { if (!HotkeyManager.TryParse(hotkey.Text, out _, out _)) { System.Windows.MessageBox.Show("단축키 형식을 확인하세요. 예: Shift + F5", "Smart Sticker"); return; } store.Save(new AppSettings { StartWithWindows = auto.IsChecked == true, RestoreNotesOnLaunch = restore.IsChecked == true, DefaultPinned = pin.IsChecked == true, CopyCaptureToClipboard = copy.IsChecked == true, CaptureShortcut = hotkey.Text, Theme = theme.SelectedItem?.ToString() ?? "Light", CaptureMode = s.CaptureMode, DefaultFontFamily = s.DefaultFontFamily, DefaultFontSize = s.DefaultFontSize }); goBack(); }; Grid.SetRow(save, 2); Children.Add(save);
    }
    private static System.Windows.Controls.CheckBox AddCheck(System.Windows.Controls.Panel parent, string text, bool value) { var check = new System.Windows.Controls.CheckBox { Content = text, IsChecked = value, Margin = new Thickness(0, 7, 0, 7) }; parent.Children.Add(check); return check; }
}
