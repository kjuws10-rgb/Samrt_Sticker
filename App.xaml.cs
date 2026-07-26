using System.Windows;

namespace SmartSticker;

public partial class App : System.Windows.Application
{
    private readonly NoteStore _store = new();
    private readonly SettingsStore _settings = new();
    private TrayManager? _tray;
    private HotkeyManager? _hotkey;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _settings.Load();
        _store.Load();
        var dashboard = new MainWindow(_store, _settings);
        _store.Changed += dashboard.RefreshNotes;
        dashboard.Show();
        _hotkey = new HotkeyManager(); _hotkey.Pressed += dashboard.CaptureNewNote; _hotkey.Register(_settings.Current.CaptureShortcut);
        _settings.SettingsSaved += settings => _hotkey.Register(settings.CaptureShortcut);
        _tray = new TrayManager(dashboard.ShowDashboard, dashboard.CreateBlankNote, dashboard.CaptureNewNote, () => ShowAllNotes(dashboard), () => ExitApplication(dashboard));
        if (_settings.Current.RestoreNotesOnLaunch)
            foreach (var note in _store.Notes) new NoteWindow(_store, note, _settings).Show();
    }

    protected override void OnExit(ExitEventArgs e) { _hotkey?.Dispose(); _tray?.Dispose(); base.OnExit(e); }
    private void ExitApplication(MainWindow dashboard) { dashboard.PrepareForExit(); Shutdown(); }
    private void ShowAllNotes(MainWindow dashboard)
    {
        foreach (var note in _store.Notes)
        {
            var existing = Current.Windows.OfType<NoteWindow>().FirstOrDefault(window => window.NoteId == note.Id);
            if (existing is null) new NoteWindow(_store, note, _settings).Show(); else { existing.Show(); existing.Activate(); }
        }
        dashboard.ShowDashboard();
    }
}
