using System.Text.Json;
using System.IO;

namespace SmartSticker;

public sealed class NoteStore
{
    private readonly string _root = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "SmartSticker");
    private string NotesFile => Path.Combine(_root, "notes.json");
    public string ImageDirectory => Path.Combine(_root, "captures");
    public List<NoteModel> Notes { get; private set; } = [];

    public List<NoteModel> Load()
    {
        try { Notes = File.Exists(NotesFile) ? JsonSerializer.Deserialize<List<NoteModel>>(File.ReadAllText(NotesFile)) ?? [] : []; }
        catch { Notes = []; }
        return Notes;
    }
    public void Add(NoteModel note) { Notes.Add(note); Save(); }
    public void Remove(NoteModel note) { Notes.Remove(note); Save(); }
    public void Save()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(NotesFile, JsonSerializer.Serialize(Notes, new JsonSerializerOptions { WriteIndented = true }));
    }
}
