using System.Windows;

namespace SmartSticker;

public partial class SettingsWindow : Window
{
    private readonly SettingsStore _store;
    public SettingsWindow(SettingsStore store)
    {
        InitializeComponent(); _store = store; var s = store.Current;
        AutoStart.IsChecked = s.StartWithWindows; RestoreNotes.IsChecked = s.RestoreNotesOnLaunch; DefaultPinned.IsChecked = s.DefaultPinned; CopyCapture.IsChecked = s.CopyCaptureToClipboard;
        CaptureShortcut.Text = s.CaptureShortcut; DefaultFontFamilyBox.Text = s.DefaultFontFamily; DefaultFontSizeBox.Text = s.DefaultFontSize.ToString();
        CaptureModeBox.SelectedIndex = (int)s.CaptureMode;
    }
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!HotkeyManager.TryParse(CaptureShortcut.Text, out _, out _)) { System.Windows.MessageBox.Show("단축키 형식이 올바르지 않습니다. 예: Shift + F5", "Smart Sticker"); return; }
        _store.Save(new AppSettings { StartWithWindows = AutoStart.IsChecked == true, RestoreNotesOnLaunch = RestoreNotes.IsChecked == true, DefaultPinned = DefaultPinned.IsChecked == true, CopyCaptureToClipboard = CopyCapture.IsChecked == true, CaptureShortcut = CaptureShortcut.Text.Trim(), DefaultFontFamily = DefaultFontFamilyBox.Text, DefaultFontSize = double.TryParse(DefaultFontSizeBox.Text, out var size) ? size : 16, CaptureMode = (CaptureMode)Math.Max(0, CaptureModeBox.SelectedIndex) });
        DialogResult = true;
    }
    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
}
