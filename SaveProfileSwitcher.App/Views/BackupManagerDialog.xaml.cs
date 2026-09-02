using System.Collections.Generic;
using System.IO;
using System.Windows;
using SaveProfileSwitcher.App.Models;
using SaveProfileSwitcher.App.Services;

namespace SaveProfileSwitcher.App.Views;

public partial class BackupManagerDialog : Window
{
    private readonly UserProfile _profile;
    private readonly GameConfig _gameConfig;
    private readonly BackupManagerService _backupManager;

    public BackupManagerDialog(UserProfile profile, GameConfig gameConfig, BackupManagerService backupManager)
    {
        InitializeComponent();
        _profile = profile;
        _gameConfig = gameConfig;
        _backupManager = backupManager;

        HeaderTextBlock.Text = "Backups for " + _gameConfig.GameName + " (" + _profile.DisplayName + ")";
        LoadBackups();
    }

    private void LoadBackups()
    {
        IList<BackupEntry> backups = _backupManager.ListBackups(_profile, _gameConfig.GameId);
        BackupsListBox.ItemsSource = backups;
    }

    private void Restore_Click(object sender, RoutedEventArgs e)
    {
        if (BackupsListBox.SelectedItem is not BackupEntry entry)
        {
            MessageBox.Show("Please select a backup to restore.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        bool snapshot = SnapshotCheckBox.IsChecked == true;

        if (MessageBox.Show(
            "Are you sure you want to restore the selected backup?\n\n" + entry.BackupFilePath,
            "Confirm Restore",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var emulator = new EmulatorDetectionService();
            var targetDir = emulator.GetTitleSaveDirectory(_gameConfig.EmulatorConfig);
            if (string.IsNullOrEmpty(targetDir))
            {
                var programData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
                var defaultRoot = System.IO.Path.Combine(programData, "SaveProfileSwitcher", "TempRestore", _gameConfig.GameId);
                targetDir = defaultRoot;
                Directory.CreateDirectory(targetDir);
            }

            _backupManager.RestoreBackup(_profile, _gameConfig, entry, targetDir, snapshot);
            MessageBox.Show("Backup restored successfully.", "Backup Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            LoggerService.Instance.LogError("Failed to restore backup from BackupManagerDialog.", ex);
            MessageBox.Show("Failed to restore backup:\n" + ex.Message, "Backup Manager", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        LoadBackups();
    }
}
