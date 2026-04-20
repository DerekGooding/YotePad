namespace YotePad.Model;

public class RecoveryFile
{
    public string RecoveryFilePath { get; set; } = string.Empty;
    public string OriginalFilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public string DisplayName => string.IsNullOrEmpty(OriginalFilePath)
        ? $"Untitled — {Timestamp:MMM d, h:mm tt}"
        : $"{Path.GetFileName(OriginalFilePath)} — {Timestamp:MMM d, h:mm tt}";
}