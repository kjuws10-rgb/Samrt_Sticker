namespace SmartSticker;

public sealed class NoteModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = "";
    public string? RtfText { get; set; }
    public string Color { get; set; } = "#FFF6B7";
    public string? ImagePath { get; set; }
    public bool IsPinned { get; set; }
    public double Left { get; set; } = 160;
    public double Top { get; set; } = 160;
    public double Width { get; set; } = 330;
    public double Height { get; set; } = 360;
    public string FontFamily { get; set; } = "맑은 고딕";
    public double FontSize { get; set; } = 16;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
