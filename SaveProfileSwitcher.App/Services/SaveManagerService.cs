using System.Diagnostics;
using System.IO;
using SaveProfileSwitcher.App.Models;

namespace SaveProfileSwitcher.App.Services;

public sealed class SaveManagerService
{
    private readonly LoggerService _logger = LoggerService.Instance;

    public void SwitchActiveProfileForGame(UserProfile profile, GameConfig game, SaveMode mode)
    {
        var emulator = new EmulatorDetectionService();
        var titleSaveDir = emulator.GetTitleSaveDirectory(game.EmulatorConfig);
        if (string.IsNullOrEmpty(titleSaveDir))
        {
            _logger.LogError("Title save directory not resolved for " + game.GameName);
            return;
        }

        var profileGameStorageDir = GetProfileGameStorageRoot(profile.Id, game.GameId);

        switch (mode)
        {
            case SaveMode.SymlinkOrJunction:
                ApplySymlinkOrJunction(profileGameStorageDir, titleSaveDir);
                break;
            case SaveMode.MoveAndSwap:
                ApplyMoveAndSwap(profileGameStorageDir, titleSaveDir);
                break;
            case SaveMode.CopyAndOverwrite:
                ApplyCopyAndOverwrite(profileGameStorageDir, titleSaveDir);
                break;
        }

        _logger.LogInfo("Switched save for " + game.GameName + " to profile " + profile.DisplayName + " using mode " + mode);
    }

    private string GetProfileGameStorageRoot(Guid profileId, string gameId)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SaveProfileSwitcher", "Profiles");
        Directory.CreateDirectory(root);
        var profileDir = Path.Combine(root, profileId.ToString());
        var gameDir = Path.Combine(profileDir, gameId);
        Directory.CreateDirectory(gameDir);
        return gameDir;
    }

    private void ApplySymlinkOrJunction(string profileStorageDir, string liveDir)
    {
        try
        {
            if (Directory.Exists(liveDir))
            {
                Directory.Delete(liveDir, true);
            }

            var parentDir = Path.GetDirectoryName(liveDir);
            if (!string.IsNullOrEmpty(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c mklink /J \"" + liveDir + "\" \"" + profileStorageDir + "\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process?.WaitForExit();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create junction for save directory.", ex);
            throw;
        }
    }

    private void ApplyMoveAndSwap(string profileStorageDir, string liveDir)
    {
        try
        {
            var inactiveDir = Path.Combine(profileStorageDir, "inactive");
            Directory.CreateDirectory(inactiveDir);

            if (Directory.Exists(liveDir))
            {
                var tempDir = Path.Combine(inactiveDir, "live_temp");
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
                Directory.Move(liveDir, tempDir);
            }

            if (Directory.Exists(profileStorageDir))
            {
                var activeDir = Path.Combine(profileStorageDir, "active");
                if (Directory.Exists(activeDir))
                {
                    Directory.Move(activeDir, liveDir);
                }
                else
                {
                    Directory.CreateDirectory(liveDir);
                }
            }
            else
            {
                Directory.CreateDirectory(liveDir);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to move and swap save directory.", ex);
            throw;
        }
    }

    private void ApplyCopyAndOverwrite(string profileStorageDir, string liveDir)
    {
        try
        {
            if (!Directory.Exists(profileStorageDir))
            {
                Directory.CreateDirectory(profileStorageDir);
            }

            if (Directory.Exists(liveDir))
            {
                Directory.Delete(liveDir, true);
            }

            CopyDirectory(profileStorageDir, liveDir);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to copy and overwrite save directory.", ex);
            throw;
        }
    }

    private void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDir, file);
            var dest = Path.Combine(targetDir, relative);
            var destDir = Path.GetDirectoryName(dest);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
            File.Copy(file, dest, true);
        }
    }
}
