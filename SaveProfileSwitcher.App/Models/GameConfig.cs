namespace SaveProfileSwitcher.App.Models;

public sealed class GameConfig
{
    public string GameId { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public SaveMode DefaultSaveMode { get; set; } = SaveMode.SymlinkOrJunction;
    public EmulatorConfig EmulatorConfig { get; set; } = new();
}
