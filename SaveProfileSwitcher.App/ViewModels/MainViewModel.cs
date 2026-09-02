using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using SaveProfileSwitcher.App.Models;
using SaveProfileSwitcher.App.Services;
using SaveProfileSwitcher.App.Views;

namespace SaveProfileSwitcher.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly SaveManagerService saveManager = new();
    private readonly BackupManagerService backupManager = new();
    private readonly string dataRootPath;
    private readonly string dataFilePath;

    private UserProfile? selectedProfile;
    private GameConfig? selectedGame;
    private string statusMessage = "Ready.";

    public ObservableCollection<UserProfile> Profiles { get; } = new();

    public ObservableCollection<GameConfig> Games { get; } = new();

    public UserProfile? SelectedProfile
    {
        get => selectedProfile;
        set
        {
            if (ReferenceEquals(selectedProfile, value))
            {
                return;
            }

            selectedProfile = value;
            OnPropertyChanged();

            if (value is not null)
            {
                StatusMessage = "Active profile: " + value.DisplayName;
                LoggerService.Instance.LogInfo(StatusMessage);
                SaveData();
            }

            CommandManager.InvalidateRequerySuggested();
        }
    }

    public GameConfig? SelectedGame
    {
        get => selectedGame;
        set
        {
            if (ReferenceEquals(selectedGame, value))
            {
                return;
            }

            selectedGame = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set
        {
            if (statusMessage == value)
            {
                return;
            }

            statusMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddProfileCommand { get; }

    public ICommand AddGameCommand { get; }

    public ICommand SwitchProfileForSelectedGameCommand { get; }

    public ICommand OpenBackupsCommand { get; }

    public ICommand CreateZipBackupCommand { get; }

    public ICommand OpenAppStorageCommand { get; }

    public MainViewModel()
    {
        dataRootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "SaveProfileSwitcher");

        dataFilePath = Path.Combine(dataRootPath, "data.json");

        AddProfileCommand = new RelayCommand(
            _ => AddProfile(),
            _ => true);

        AddGameCommand = new RelayCommand(
            _ => AddGame(),
            _ => true);

        SwitchProfileForSelectedGameCommand = new RelayCommand(
            _ => SwitchProfileForSelectedGame(),
            _ => SelectedProfile is not null && SelectedGame is not null);

        OpenBackupsCommand = new RelayCommand(
            _ => OpenBackups(),
            _ => SelectedProfile is not null && SelectedGame is not null);

        CreateZipBackupCommand = new RelayCommand(
            _ => CreateZipBackup(),
            _ => SelectedProfile is not null && SelectedGame is not null);

        OpenAppStorageCommand = new RelayCommand(
            _ => OpenAppStorage(),
            _ => true);

        Directory.CreateDirectory(dataRootPath);

        LoadOrCreateData();
    }

    public void AddProfileFromDialog(UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (profile.Id == Guid.Empty)
        {
            profile.Id = Guid.NewGuid();
        }

        if (string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            throw new ArgumentException("Profile display name is required.", nameof(profile));
        }

        if (string.IsNullOrWhiteSpace(profile.StorageRootPath))
        {
            profile.StorageRootPath = Path.Combine(
                dataRootPath,
                "Profiles",
                profile.Id.ToString());
        }

        Directory.CreateDirectory(profile.StorageRootPath);

        Profiles.Add(profile);
        SelectedProfile = profile;

        SaveData();

        StatusMessage = "Created profile: " + profile.DisplayName;
        LoggerService.Instance.LogInfo(StatusMessage);
    }

    public void AddGameFromDialog(GameConfig game)
    {
        ArgumentNullException.ThrowIfNull(game);

        if (string.IsNullOrWhiteSpace(game.GameId))
        {
            game.GameId = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrWhiteSpace(game.GameName))
        {
            throw new ArgumentException("Game title is required.", nameof(game));
        }

        game.EmulatorConfig ??= new EmulatorConfig();

        Games.Add(game);
        SelectedGame = game;

        SaveData();

        StatusMessage = "Added game: " + game.GameName;
        LoggerService.Instance.LogInfo(StatusMessage);
    }

    public void SaveData()
    {
        try
        {
            Directory.CreateDirectory(dataRootPath);

            var data = new PersistedApplicationData
            {
                ActiveProfileId = SelectedProfile?.Id,
                Profiles = Profiles.ToList(),
                Games = Games.ToList()
            };

            string json = JsonSerializer.Serialize(data, JsonOptions);
            string temporaryPath = dataFilePath + ".tmp";

            File.WriteAllText(temporaryPath, json);

            if (File.Exists(dataFilePath))
            {
                File.Delete(dataFilePath);
            }

            File.Move(temporaryPath, dataFilePath);
        }
        catch (Exception ex)
        {
            LoggerService.Instance.LogError("Failed to persist application data.", ex);
            StatusMessage = "Failed to save data. Check logs.";
        }
    }

    private void LoadOrCreateData()
    {
        try
        {
            if (!File.Exists(dataFilePath))
            {
                CreateFirstRunData();
                SaveData();
                return;
            }

            string json = File.ReadAllText(dataFilePath);

            PersistedApplicationData? data = JsonSerializer.Deserialize<PersistedApplicationData>(
                json,
                JsonOptions);

            if (data is null)
            {
                CreateFirstRunData();
                SaveData();
                return;
            }

            Profiles.Clear();
            Games.Clear();

            foreach (UserProfile profile in data.Profiles)
            {
                if (profile.Id == Guid.Empty)
                {
                    profile.Id = Guid.NewGuid();
                }

                if (string.IsNullOrWhiteSpace(profile.StorageRootPath))
                {
                    profile.StorageRootPath = Path.Combine(
                        dataRootPath,
                        "Profiles",
                        profile.Id.ToString());
                }

                Directory.CreateDirectory(profile.StorageRootPath);
                Profiles.Add(profile);
            }

            foreach (GameConfig game in data.Games)
            {
                game.EmulatorConfig ??= new EmulatorConfig();

                if (string.IsNullOrWhiteSpace(game.GameId))
                {
                    game.GameId = Guid.NewGuid().ToString("N");
                }

                Games.Add(game);
            }

            if (Profiles.Count == 0)
            {
                CreateFirstRunData();
                SaveData();
                return;
            }

            SelectedProfile = Profiles.FirstOrDefault(profile => profile.Id == data.ActiveProfileId)
                ?? Profiles.First();

            SelectedGame = Games.FirstOrDefault();

            StatusMessage = "Loaded saved profiles and games.";
            LoggerService.Instance.LogInfo(StatusMessage);
        }
        catch (Exception ex)
        {
            LoggerService.Instance.LogError("Failed to load application data. Recreating defaults.", ex);

            Profiles.Clear();
            Games.Clear();

            CreateFirstRunData();
            SaveData();
        }
    }

    private void CreateFirstRunData()
    {
        Profiles.Clear();
        Games.Clear();

        var primaryProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "Primary Account"
        };

        primaryProfile.StorageRootPath = Path.Combine(
            dataRootPath,
            "Profiles",
            primaryProfile.Id.ToString());

        Directory.CreateDirectory(primaryProfile.StorageRootPath);

        Profiles.Add(primaryProfile);
        SelectedProfile = primaryProfile;

        Games.Add(new GameConfig
        {
            GameId = "AC_ODYSSEY",
            GameName = "Assassin's Creed Odyssey",
            DefaultSaveMode = SaveMode.SymlinkOrJunction,
            EmulatorConfig = new EmulatorConfig
            {
                EmulatorType = EmulatorType.Unknown,
                PlatformName = "PC",
                TitleId = "ACOD",
                ExecutablePath = "C:/Games/ACOdyssey/ACOdyssey.exe",
                SaveRootPath = "C:/Saves/ACOdyssey"
            }
        });

        Games.Add(new GameConfig
        {
            GameId = "FH5",
            GameName = "Forza Horizon 5",
            DefaultSaveMode = SaveMode.MoveAndSwap,
            EmulatorConfig = new EmulatorConfig
            {
                EmulatorType = EmulatorType.Unknown,
                PlatformName = "PC",
                TitleId = "FH5",
                ExecutablePath = "C:/Games/FH5/ForzaHorizon5.exe",
                SaveRootPath = "C:/Saves/FH5"
            }
        });

        Games.Add(new GameConfig
        {
            GameId = "RPCS3_BLUS30109",
            GameName = "Ninja Gaiden Sigma 2 (RPCS3)",
            DefaultSaveMode = SaveMode.CopyAndOverwrite,
            EmulatorConfig = new EmulatorConfig
            {
                EmulatorType = EmulatorType.RPCS3,
                PlatformName = "PlayStation 3",
                TitleId = "BLUS30109",
                ExecutablePath = "C:/Emulators/RPCS3/rpcs3.exe",
                SaveRootPath = "C:/Emulators/RPCS3/dev_hdd0/game"
            }
        });

        SelectedGame = Games.FirstOrDefault();

        StatusMessage = "Created first-run application data.";
        LoggerService.Instance.LogInfo(StatusMessage);
    }

    private void AddProfile()
    {
        var dialog = new AddProfileDialog
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        AddProfileFromDialog(dialog.CreatedProfile);
    }

    private void AddGame()
    {
        var dialog = new AddGameDialog
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        AddGameFromDialog(dialog.CreatedGame);
    }

    private void SwitchProfileForSelectedGame()
    {
        if (SelectedProfile is null || SelectedGame is null)
        {
            return;
        }

        try
        {
            saveManager.SwitchActiveProfileForGame(
                SelectedProfile,
                SelectedGame,
                SelectedGame.DefaultSaveMode);

            StatusMessage = "Switched save for " + SelectedGame.GameName;
        }
        catch (Exception ex)
        {
            LoggerService.Instance.LogError("Failed to switch profile for game.", ex);
            StatusMessage = "Save switch failed. Check logs.";
        }
    }

    private void OpenBackups()
    {
        if (SelectedProfile is null || SelectedGame is null)
        {
            return;
        }

        var dialog = new BackupManagerDialog(
            SelectedProfile,
            SelectedGame,
            backupManager)
        {
            Owner = Application.Current.MainWindow
        };

        dialog.ShowDialog();
    }

    private void CreateZipBackup()
    {
        if (SelectedProfile is null || SelectedGame is null)
        {
            return;
        }

        try
        {
            var emulatorService = new EmulatorDetectionService();
            string liveSavePath = emulatorService.GetTitleSaveDirectory(
                SelectedGame.EmulatorConfig);

            if (string.IsNullOrWhiteSpace(liveSavePath))
            {
                MessageBox.Show(
                    "The selected game does not have a valid live save path.",
                    "SaveProfileSwitcher",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            BackupEntry backup = backupManager.CreateBackup(
                SelectedProfile,
                SelectedGame,
                liveSavePath);

            StatusMessage = "Created backup: " + backup.BackupFilePath;

            MessageBox.Show(
                "ZIP backup created successfully.",
                "SaveProfileSwitcher",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            LoggerService.Instance.LogError("Failed to create ZIP backup.", ex);

            MessageBox.Show(
                "Failed to create backup: " + ex.Message,
                "SaveProfileSwitcher",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OpenAppStorage()
    {
        try
        {
            Directory.CreateDirectory(dataRootPath);

            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "\"" + dataRootPath + "\"",
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            LoggerService.Instance.LogError("Failed to open app storage directory.", ex);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    private sealed class PersistedApplicationData
    {
        public Guid? ActiveProfileId { get; set; }

        public List<UserProfile> Profiles { get; set; } = new();

        public List<GameConfig> Games { get; set; } = new();
    }

    private sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> execute;
        private readonly Predicate<object?>? canExecute;

        public RelayCommand(
            Action<object?> execute,
            Predicate<object?>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
