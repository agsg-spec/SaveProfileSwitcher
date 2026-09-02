using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using SaveProfileSwitcher.App.Models;
using SaveProfileSwitcher.App.Services;
using SaveProfileSwitcher.App.Views;

namespace SaveProfileSwitcher.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SaveManagerService _saveManager = new();
    private readonly BackupManagerService _backupManager = new();
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

    public ICommand SwitchProfileForSelectedGameCommand { get; }
    public ICommand OpenBackupsCommand { get; }

    public MainViewModel()
    {
        SwitchProfileForSelectedGameCommand = new RelayCommand(
            _ => SwitchProfileForSelectedGame(),
            _ => SelectedProfile != null && SelectedGame != null);

        OpenBackupsCommand = new RelayCommand(
            _ => OpenBackups(),
            _ => SelectedProfile != null && SelectedGame != null);

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

        Games.Add(new GameConfig { GameId = "AC_ODYSSEY", GameName = "Assassin's Creed Odyssey", DefaultSaveMode = SaveMode.SymlinkOrJunction, EmulatorConfig = new EmulatorConfig { PlatformName = "PC", TitleId = "ACOD", ExecutablePath = "C:/Games/ACOdyssey/ACOdyssey.exe", SaveRootPath = "C:/Saves/ACOdyssey" } });
        Games.Add(new GameConfig { GameId = "FH5", GameName = "Forza Horizon 5", DefaultSaveMode = SaveMode.MoveAndSwap, EmulatorConfig = new EmulatorConfig { PlatformName = "PC", TitleId = "FH5", ExecutablePath = "C:/Games/FH5/ForzaHorizon5.exe", SaveRootPath = "C:/Saves/FH5" } });
        Games.Add(new GameConfig { GameId = "RPCS3_BLUS30109", GameName = "Ninja Gaiden Sigma 2 (RPCS3)", DefaultSaveMode = SaveMode.CopyAndOverwrite, EmulatorConfig = new EmulatorConfig { EmulatorType = EmulatorType.RPCS3, PlatformName = "PlayStation 3", TitleId = "BLUS30109", ExecutablePath = "C:/Emulators/RPCS3/rpcs3.exe", SaveRootPath = "C:/Emulators/RPCS3/dev_hdd0/game" } });
        SelectedGame = Games[0];
    }

    private void SwitchProfileForSelectedGame()
    {
        if (SelectedProfile == null || SelectedGame == null) return;
        try
        {
            _saveManager.SwitchActiveProfileForGame(SelectedProfile, SelectedGame, SelectedGame.DefaultSaveMode);
        }
        catch (Exception ex)
        {
            LoggerService.Instance.LogError("Failed to switch profile for game.", ex);
        }
    }

    private void OpenBackups()
    {
        if (SelectedProfile == null || SelectedGame == null) return;
        var dialog = new BackupManagerDialog(SelectedProfile, SelectedGame, _backupManager)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) { _execute = execute; _canExecute = canExecute; }
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }
    }
}
