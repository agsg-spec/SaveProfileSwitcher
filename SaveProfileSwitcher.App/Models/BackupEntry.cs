namespace SaveProfileSwitcher.App.Models;

public sealed class BackupEntry
{
    public string BackupFilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ProfileId { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;

    public override string ToString()
    {
        return CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") + " - " + BackupFilePath;
    }
}
