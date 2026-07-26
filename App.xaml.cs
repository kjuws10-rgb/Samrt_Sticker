using System.Windows;

namespace SmartSticker;

public partial class App : System.Windows.Application
{
    private readonly NoteStore _store = new();
    private readonly SettingsStore _settings = new();
    private TrayManager? _tray;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _settings.Load();
        var dashboard = new MainWindow(_store, _settings);
        dashboard.Show();
        _tray = new TrayManager(dashboard.ShowDashboard, dashboard.CreateBlankNote, dashboard.CaptureNewNote, () => ExitApplication(dashboard));
        if (_settings.Current.RestoreNotesOnLaunch)
            foreach (var note in _store.Load()) new NoteWindow(_store, note).Show();
    }

    protected override void OnExit(ExitEventArgs e) { _tray?.Dispose(); base.OnExit(e); }
    private void ExitApplication(MainWindow dashboard) { dashboard.PrepareForExit(); Shutdown(); }
}
