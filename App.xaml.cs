using System.Windows;

namespace SmartSticker;

public partial class App : System.Windows.Application
{
    private readonly NoteStore _store = new();
    private readonly SettingsStore _settings = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _settings.Load();
        var dashboard = new MainWindow(_store, _settings);
        dashboard.Show();
        if (_settings.Current.RestoreNotesOnLaunch)
            foreach (var note in _store.Load()) new NoteWindow(_store, note).Show();
    }
}
