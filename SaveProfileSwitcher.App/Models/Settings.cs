namespace SaveProfileSwitcher.App.Models;

public sealed class Settings
{
    public bool EnableAutomaticBackups { get; set; } = true;
    public int MaxBackupsPerGame { get; set; } = 10;
    public string DefaultBackupLocation { get; set; } = string.Empty;
    public bool EnableLogging { get; set; } = true;
    public int LogRetentionDays { get; set; } = 30;
    public bool DarkMode { get; set; }
}
