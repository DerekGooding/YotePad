namespace YotePad.Services;

public class RecoveryService : IDisposable
{
    private static readonly string _recoveryFolder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "YannPerodin", "YotePad", "Recovery"
);

    private readonly string _recoveryFilePath;
    private readonly string _lockFilePath;
    private FileStream? _lockStream = null;
    private bool _hasWrittenRecovery = false;

    public RecoveryService()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var pid = Environment.ProcessId;
        _recoveryFilePath = Path.Combine(_recoveryFolder, $"recovery_{timestamp}_{pid}.ypr");
        _lockFilePath = _recoveryFilePath + ".lock";

        Directory.CreateDirectory(_recoveryFolder);

        try
        {
            _lockStream = new FileStream(
                _lockFilePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None
            );
        }
        catch { }
    }

    public void WriteRecoveryFile(string content, string originalPath)
    {
        try
        {
            var header = $"YOTEPAD_RECOVERY|{originalPath}";
            var fullContent = header + "\n" + content;
            File.WriteAllText(_recoveryFilePath, fullContent);
            _hasWrittenRecovery = true;
        }
        catch { }
    }

    public async Task WriteRecoveryFileAsync(string content, string originalPath)
    {
        try
        {
            var header = $"YOTEPAD_RECOVERY|{originalPath}";
            var fullContent = header + "\n" + content;

            // This is the magic line. It writes the file without blocking the UI.
            await File.WriteAllTextAsync(_recoveryFilePath, fullContent);

            _hasWrittenRecovery = true;
        }
        catch { }
    }

    public void DeleteRecoveryFile()
    {
        try
        {
            _lockStream?.Close();
            _lockStream?.Dispose();
            _lockStream = null;

            if (File.Exists(_lockFilePath))
                File.Delete(_lockFilePath);

            if (_hasWrittenRecovery && File.Exists(_recoveryFilePath))
                File.Delete(_recoveryFilePath);
        }
        catch { }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _lockStream?.Close();
        _lockStream?.Dispose();
        _lockStream = null;
    }

    public static RecoveryFile[] ScanForRecoveryFiles()
    {
        try
        {
            if (!Directory.Exists(_recoveryFolder)) return [];

            foreach (var orphan in Directory.GetFiles(_recoveryFolder, "*.ypr.restoring"))
            {
                try { File.Delete(orphan); } catch { }
            }

            foreach (var lockFile in Directory.GetFiles(_recoveryFolder, "*.ypr.lock"))
            {
                if (!IsFileLocked(lockFile))
                {
                    try { File.Delete(lockFile); } catch { }
                }
            }

            var files = Directory.GetFiles(_recoveryFolder, "*.ypr");
            var results = new List<RecoveryFile>();

            foreach (var file in files)
            {
                try
                {
                    var lockPath = file + ".lock";
                    if (File.Exists(lockPath) && IsFileLocked(lockPath)) continue;

                    var raw = File.ReadAllText(file);
                    var newline = raw.IndexOf('\n');
                    if (newline == -1) continue;

                    var header = raw[..newline];
                    var content = raw[(newline + 1)..];

                    if (!header.StartsWith("YOTEPAD_RECOVERY|")) continue;

                    var originalPath = header["YOTEPAD_RECOVERY|".Length..];

                    results.Add(new RecoveryFile
                    (
                        file,
                        originalPath,
                        content,
                        File.GetLastWriteTime(file)
                    ));
                }
                catch { }
            }

            return [.. results];
        }
        catch { return []; }
    }

    public static void DeleteRecoveryFileAt(string path)
    {
        try { File.Delete(path); } catch { }
        try { File.Delete(path + ".lock"); } catch { }
    }

    private static bool IsFileLocked(string filePath)
    {
        try
        {
            using var fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }
}
