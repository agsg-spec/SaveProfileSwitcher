using System.IO;
using System.IO.Compression;
using SaveProfileSwitcher.App.Models;

namespace SaveProfileSwitcher.App.Services;

public sealed class BackupManagerService
{
    private readonly LoggerService _logger = LoggerService.Instance;
    private readonly int _retentionCount = 3;

    public BackupEntry CreateBackup(UserProfile profile, GameConfig game, string liveSaveDirectory)
    {
        var backupDir = GetBackupFolder(profile.Id, game.GameId);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var zipPath = Path.Combine(backupDir, "Save_" + timestamp + ".zip");

        try
        {
            if (Directory.Exists(liveSaveDirectory))
            {
                ZipFile.CreateFromDirectory(liveSaveDirectory, zipPath, CompressionLevel.Optimal, false);
            }
            else
            {
                throw new DirectoryNotFoundException("Live save directory not found: " + liveSaveDirectory);
            }

            var entry = new BackupEntry
            {
                BackupFilePath = zipPath,
                CreatedAt = DateTime.Now,
                ProfileId = profile.Id.ToString(),
                GameId = game.GameId
            };

            PurgeOldBackups(profile.Id, game.GameId);
            _logger.LogInfo("Created backup for " + game.GameName + " at " + zipPath);
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create backup.", ex);
            throw;
        }
    }

    public IList<BackupEntry> ListBackups(UserProfile profile, string gameId)
    {
        var result = new List<BackupEntry>();
        var backupDir = GetBackupFolder(profile.Id, gameId);
        if (!Directory.Exists(backupDir))
        {
            return result;
        }

        foreach (string file in Directory.GetFiles(backupDir, "Save_*.zip"))
        {
            var info = new FileInfo(file);
            result.Add(new BackupEntry
            {
                BackupFilePath = file,
                CreatedAt = info.CreationTime,
                ProfileId = profile.Id.ToString(),
                GameId = gameId
            });
        }

        return result.OrderByDescending(b => b.CreatedAt).ToList();
    }

    public void RestoreBackup(UserProfile profile, GameConfig game, BackupEntry entry, string targetDirectory, bool createSnapshotBackup)
    {
        try
        {
            if (!File.Exists(entry.BackupFilePath))
            {
                throw new FileNotFoundException("Backup file not found.", entry.BackupFilePath);
            }

            if (createSnapshotBackup && Directory.Exists(targetDirectory))
            {
                CreateBackup(profile, game, targetDirectory);
            }

            if (Directory.Exists(targetDirectory))
            {
                Directory.Delete(targetDirectory, true);
            }

            Directory.CreateDirectory(targetDirectory);
            ZipFile.ExtractToDirectory(entry.BackupFilePath, targetDirectory);
            _logger.LogInfo("Restored backup " + entry.BackupFilePath + " to " + targetDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to restore backup.", ex);
            throw;
        }
    }

    private string GetBackupFolder(Guid profileId, string gameId)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SaveProfileSwitcher", "Backups");
        Directory.CreateDirectory(root);
        var profileDir = Path.Combine(root, profileId.ToString());
        var gameDir = Path.Combine(profileDir, gameId);
        Directory.CreateDirectory(gameDir);
        return gameDir;
    }

    private void PurgeOldBackups(Guid profileId, string gameId)
    {
        var backups = ListBackups(new UserProfile { Id = profileId }, gameId);
        if (backups.Count <= _retentionCount)
        {
            return;
        }

        var toDelete = backups.OrderBy(b => b.CreatedAt).Take(backups.Count - _retentionCount).ToList();
        foreach (var backup in toDelete)
        {
            try
            {
                File.Delete(backup.BackupFilePath);
                _logger.LogInfo("Deleted old backup: " + backup.BackupFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete backup " + backup.BackupFilePath, ex);
            }
        }
    }
}
