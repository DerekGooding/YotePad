namespace YotePad.Model;

public readonly record struct RecoveryFile(string RecoveryFilePath, string OriginalFilePath, string Content, DateTime Timestamp)
{
    public readonly string DisplayName => string.IsNullOrEmpty(OriginalFilePath)
        ? $"Untitled — {Timestamp:MMM d, h:mm tt}"
        : $"{Path.GetFileName(OriginalFilePath)} — {Timestamp:MMM d, h:mm tt}";
}