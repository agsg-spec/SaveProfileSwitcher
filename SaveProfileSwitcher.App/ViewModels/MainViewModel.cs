using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SaveProfileSwitcher.App.Models;
using SaveProfileSwitcher.App.Services;

namespace SaveProfileSwitcher.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private UserProfile? selectedProfile;
    private GameConfig? selectedGame;

    public ObservableCollection<UserProfile> Profiles { get; } = new();
    public ObservableCollection<GameConfig> Games { get; } = new();

    public UserProfile? SelectedProfile
    {
        get => selectedProfile;
        set
        {
            if (selectedProfile == value) return;
            selectedProfile = value;
            OnPropertyChanged();
            LoggerService.Instance.LogInfo("Active profile: " + (value?.DisplayName ?? "None"));
        }
    }

    public GameConfig? SelectedGame
    {
        get => selectedGame;
        set
        {
            if (selectedGame == value) return;
            selectedGame = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel()
    {
        var profilesRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SaveProfileSwitcher", "Profiles");
        Directory.CreateDirectory(profilesRoot);
        var profile = new UserProfile
        {
            DisplayName = "Primary Account",
            StorageRootPath = Path.Combine(profilesRoot, Guid.NewGuid().ToString())
        };
        Directory.CreateDirectory(profile.StorageRootPath);
        Profiles.Add(profile);
        SelectedProfile = profile;

        Games.Add(new GameConfig { GameId = "AC_ODYSSEY", GameName = "Assassin's Creed Odyssey", DefaultSaveMode = SaveMode.SymlinkOrJunction, EmulatorConfig = new EmulatorConfig { PlatformName = "PC", TitleId = "ACOD", ExecutablePath = "C:/Games/ACOdyssey/ACOdyssey.exe" } });
        Games.Add(new GameConfig { GameId = "FH5", GameName = "Forza Horizon 5", DefaultSaveMode = SaveMode.MoveAndSwap, EmulatorConfig = new EmulatorConfig { PlatformName = "PC", TitleId = "FH5", ExecutablePath = "C:/Games/FH5/ForzaHorizon5.exe" } });
        Games.Add(new GameConfig { GameId = "RPCS3_BLUS30109", GameName = "Ninja Gaiden Sigma 2 (RPCS3)", DefaultSaveMode = SaveMode.CopyAndOverwrite, EmulatorConfig = new EmulatorConfig { EmulatorType = EmulatorType.RPCS3, PlatformName = "PlayStation 3", TitleId = "BLUS30109", ExecutablePath = "C:/Emulators/RPCS3/rpcs3.exe" } });
        SelectedGame = Games[0];
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
