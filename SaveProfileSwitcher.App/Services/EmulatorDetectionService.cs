using System.IO;
using SaveProfileSwitcher.App.Models;

namespace SaveProfileSwitcher.App.Services;

public sealed class EmulatorDetectionService
{
    public EmulatorType DetectEmulatorType(string executablePath)
    {
        var fileName = Path.GetFileName(executablePath).ToLowerInvariant();
        return fileName switch
        {
            "xenia.exe" => EmulatorType.Xenia,
            "rpcs3.exe" => EmulatorType.RPCS3,
            "cemu.exe" => EmulatorType.Cemu,
            "pcsx2.exe" => EmulatorType.PCSX2,
            "dolphin.exe" => EmulatorType.Dolphin,
            "duckstation.exe" => EmulatorType.DuckStation,
            "ryujinx.exe" => EmulatorType.Ryujinx,
            _ => EmulatorType.Unknown
        };
    }

    public string GetTitleSaveDirectory(EmulatorConfig config)
    {
        if (string.IsNullOrEmpty(config.SaveRootPath) || string.IsNullOrEmpty(config.TitleId))
        {
            return string.Empty;
        }

        return config.EmulatorType switch
        {
            EmulatorType.Xenia => Path.Combine(config.SaveRootPath, config.TitleId),
            EmulatorType.RPCS3 => Path.Combine(config.SaveRootPath, config.TitleId),
            EmulatorType.Cemu => Path.Combine(config.SaveRootPath, "usr", "save", config.TitleId),
            EmulatorType.Ryujinx => Path.Combine(config.SaveRootPath, config.TitleId),
            _ => Path.Combine(config.SaveRootPath, config.TitleId)
        };
    }
}
