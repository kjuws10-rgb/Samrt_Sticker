using System.Windows;

namespace SmartSticker;

public partial class SettingsWindow : Window
{
    private readonly SettingsStore _store;
    public SettingsWindow(SettingsStore store)
    {
        InitializeComponent(); _store = store; var s = store.Current;
        AutoStart.IsChecked = s.StartWithWindows; RestoreNotes.IsChecked = s.RestoreNotesOnLaunch; DefaultPinned.IsChecked = s.DefaultPinned; CopyCapture.IsChecked = s.CopyCaptureToClipboard;
    }
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _store.Save(new AppSettings { StartWithWindows = AutoStart.IsChecked == true, RestoreNotesOnLaunch = RestoreNotes.IsChecked == true, DefaultPinned = DefaultPinned.IsChecked == true, CopyCaptureToClipboard = CopyCapture.IsChecked == true });
        DialogResult = true;
    }
    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
}
