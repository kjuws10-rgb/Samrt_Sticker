using System.Windows;

namespace SmartSticker;

public partial class App : System.Windows.Application
{
    private static Mutex? _instance;
    private readonly NoteStore _store = new();
    private readonly SettingsStore _settings = new();
    private TrayManager? _tray;
    private HotkeyManager? _captureHotkey;
    private HotkeyManager? _showAllNotesHotkey;

    protected override void OnStartup(StartupEventArgs e)
    {
        DpiAwareness.EnablePerMonitorV2();
        _instance = new Mutex(true, "SmartSticker.SingleInstance", out var created);
        if (!created) { Shutdown(); return; }
        base.OnStartup(e);
        _settings.Load();
        Resources["AppTheme"] = _settings.Current.Theme;
        _store.Load();
        var dashboard = new MainWindow(_store, _settings);
        _store.Changed += dashboard.RefreshNotes;
        dashboard.Show();
        _captureHotkey = new HotkeyManager(45017); _captureHotkey.Pressed += dashboard.CaptureNewNote; _captureHotkey.Register(_settings.Current.CaptureShortcut);
        _showAllNotesHotkey = new HotkeyManager(45018); _showAllNotesHotkey.Pressed += () => ShowAllNotes(dashboard); _showAllNotesHotkey.Register(_settings.Current.ShowAllNotesShortcut);
        _settings.SettingsSaved += settings =>
        {
            _captureHotkey.ClearRegistration();
            _showAllNotesHotkey.ClearRegistration();
            var captureRegistered = _captureHotkey.Register(settings.CaptureShortcut);
            var showAllRegistered = _showAllNotesHotkey.Register(settings.ShowAllNotesShortcut);
            if (!captureRegistered || !showAllRegistered)
                System.Windows.MessageBox.Show("다른 프로그램에서 사용 중인 단축키는 등록할 수 없습니다. 사용 중이지 않은 조합으로 변경해 주세요.", "Smart Sticker");
        };
        _settings.SettingsSaved += settings => dashboard.ApplyTheme(settings.Theme);
        _tray = new TrayManager(dashboard.ShowDashboard, dashboard.CreateBlankNote, dashboard.CaptureNewNote, () => ShowAllNotes(dashboard), () => HideAllNotes(dashboard), () => ExitApplication(dashboard));
        if (_settings.Current.RestoreNotesOnLaunch)
            foreach (var note in _store.Notes) new NoteWindow(_store, note, _settings).Show();
        if (e.Args.Contains("--new-note")) dashboard.CreateBlankNote();
        if (e.Args.Contains("--show-all")) ShowAllNotes(dashboard);
    }

    protected override void OnExit(ExitEventArgs e) { _captureHotkey?.Dispose(); _showAllNotesHotkey?.Dispose(); _tray?.Dispose(); base.OnExit(e); }
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
    private static void HideAllNotes(MainWindow dashboard)
    {
        foreach (var note in Current.Windows.OfType<NoteWindow>().ToList()) note.Hide();
        dashboard.Hide();
    }
}
