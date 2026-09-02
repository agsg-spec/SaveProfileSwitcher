namespace SaveProfileSwitcher.App.Models;

public sealed class Settings
{
    public bool EnableAutomaticBackups { get; set; } = true;
    public int MaxBackupsPerGame { get; set; } = 3;
    public string DefaultBackupLocation { get; set; } = string.Empty;
    public bool EnableLogging { get; set; } = true;
    public int LogRetentionDays { get; set; } = 30;
    public bool DarkMode { get; set; } = true;
    public SaveMode GlobalDefaultSaveMode { get; set; } = SaveMode.CopyAndOverwrite;
    public bool SafetyStagingEnabled { get; set; } = true;
    public bool ConfirmBeforeSwitching { get; set; } = true;
    public bool PreventSwitchWhileGameRunning { get; set; } = true;
    public bool BackupBeforeSwitching { get; set; } = true;
    public bool BackupBeforeRestoring { get; set; } = true;
    public bool BackupBeforeOverwrite { get; set; } = true;
    public bool EnableAutomaticExeDetection { get; set; } = true;
    public bool SearchPortableEmulatorFolders { get; set; } = true;
    public bool SearchAppDataAndDocuments { get; set; } = true;
    public string ApplicationRoot { get; set; } = string.Empty;
    public string ProfilesRoot { get; set; } = string.Empty;
    public string BackupsRoot { get; set; } = string.Empty;
    public string LogsRoot { get; set; } = string.Empty;
    public string CustomEmulatorExecutable { get; set; } = string.Empty;
    public int TextSize { get; set; } = 14;
    public bool CompactLayout { get; set; }
    public bool ShowGameIds { get; set; } = true;
    public bool ShowSavePaths { get; set; } = true;
}
