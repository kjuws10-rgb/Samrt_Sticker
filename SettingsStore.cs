using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.IO;

namespace SmartSticker;

public sealed class AppSettings
{
    public bool StartWithWindows { get; set; }
    public bool RestoreNotesOnLaunch { get; set; } = true;
    public bool DefaultPinned { get; set; }
    public bool CopyCaptureToClipboard { get; set; } = true;
    public string CaptureShortcut { get; set; } = "Shift + F5";
    public string DefaultFontFamily { get; set; } = "맑은 고딕";
    public double DefaultFontSize { get; set; } = 16;
    public CaptureMode CaptureMode { get; set; } = CaptureMode.FullScreen;
}

public enum CaptureMode { FullScreen, Rectangle, Freeform, Scroll }

public sealed class SettingsStore
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "SmartSticker";
    private readonly string _file = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), AppName, "settings.json");
    public AppSettings Current { get; private set; } = new();
    public event Action<AppSettings>? SettingsSaved;

    public void Load()
    {
        try { Current = File.Exists(_file) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_file)) ?? new() : new(); }
        catch { Current = new(); }
        Current.StartWithWindows = IsAutoStartEnabled();
    }
    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
        File.WriteAllText(_file, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        SetAutoStart(settings.StartWithWindows); Current = settings; SettingsSaved?.Invoke(Current);
    }
    private static bool IsAutoStartEnabled() => Registry.CurrentUser.OpenSubKey(RunKey)?.GetValue(AppName) is string;
    private static void SetAutoStart(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled) key.SetValue(AppName, BuildLaunchCommand()); else key.DeleteValue(AppName, false);
    }
    private static string BuildLaunchCommand()
    {
        var processPath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? "SmartSticker.exe";
        var entryAssembly = Assembly.GetEntryAssembly()?.Location;
        return processPath.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(entryAssembly)
            ? $"\"{processPath}\" \"{entryAssembly}\""
            : $"\"{processPath}\"";
    }
}
