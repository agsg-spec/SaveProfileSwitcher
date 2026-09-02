using System;
using System.IO;
using System.Text.Json;
using SaveProfileSwitcher.App.Models;
namespace SaveProfileSwitcher.App.Services;
public sealed class SettingsService
{
    private static readonly Lazy<SettingsService> LazyInstance = new(() => new SettingsService());
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly string settingsPath;
    public static SettingsService Instance => LazyInstance.Value;
    private SettingsService() { string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SaveProfileSwitcher"); Directory.CreateDirectory(root); settingsPath = Path.Combine(root, "settings.json"); }
    public Settings LoadSettings() { try { return File.Exists(settingsPath) ? JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath), JsonOptions) ?? CreateDefaults() : CreateDefaults(); } catch (Exception ex) { LoggerService.Instance.LogError("Failed to load settings.", ex); return CreateDefaults(); } }
    public void SaveSettings(Settings settings) { ArgumentNullException.ThrowIfNull(settings); string temporaryPath = settingsPath + ".tmp"; File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, JsonOptions)); File.Move(temporaryPath, settingsPath, true); }
    private static Settings CreateDefaults() => new() { DefaultBackupLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SaveProfileSwitcher", "Backups") };
}
