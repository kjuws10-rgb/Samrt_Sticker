using System.Windows;

namespace SmartSticker;

public partial class App : System.Windows.Application
{
    private readonly NoteStore _store = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var dashboard = new MainWindow(_store);
        dashboard.Show();
        foreach (var note in _store.Load()) new NoteWindow(_store, note).Show();
    }
}
