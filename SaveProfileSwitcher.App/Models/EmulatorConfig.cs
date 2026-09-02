namespace SaveProfileSwitcher.App.Models;

public sealed class EmulatorConfig
{
    public EmulatorType EmulatorType { get; set; }
    public string ExecutablePath { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
    public string TitleId { get; set; } = string.Empty;
}
