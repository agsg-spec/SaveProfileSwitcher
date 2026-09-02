using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly SaveManagerService saveManager = new();
    private readonly BackupManagerService backupManager = new();
    private readonly string dataRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SaveProfileSwitcher");
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
            if (ReferenceEquals(selectedProfile, value)) return;
            selectedProfile = value;
            OnPropertyChanged();
            if (value is not null) SaveData();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public GameConfig? SelectedGame
    {
        get => selectedGame;
        set
        {
            if (ReferenceEquals(selectedGame, value)) return;
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
            if (statusMessage == value) return;
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
        dataFilePath = Path.Combine(dataRootPath, "data.json");
        AddProfileCommand = new RelayCommand(_ => AddProfile());
        AddGameCommand = new RelayCommand(_ => AddGame());
        SwitchProfileForSelectedGameCommand = new RelayCommand(_ => SwitchProfileForSelectedGame(), _ => SelectedProfile is not null && SelectedGame is not null);
        OpenBackupsCommand = new RelayCommand(_ => OpenBackups(), _ => SelectedProfile is not null && SelectedGame is not null);
        CreateZipBackupCommand = new RelayCommand(_ => CreateZipBackup(), _ => SelectedProfile is not null && SelectedGame is not null);
        OpenAppStorageCommand = new RelayCommand(_ => OpenAppStorage());

        Directory.CreateDirectory(dataRootPath);
        LoadOrCreateData();
    }

    public void AddProfileFromDialog(UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (profile.Id == Guid.Empty) profile.Id = Guid.NewGuid();
        if (string.IsNullOrWhiteSpace(profile.StorageRootPath))
            profile.StorageRootPath = Path.Combine(dataRootPath, "Profiles", profile.Id.ToString());
        Directory.CreateDirectory(profile.StorageRootPath);
        Profiles.Add(profile);
        SelectedProfile = profile;
        SaveData();
    }

    public void AddGameFromDialog(GameConfig game)
    {
        ArgumentNullException.ThrowIfNull(game);
        if (string.IsNullOrWhiteSpace(game.GameId)) game.GameId = Guid.NewGuid().ToString("N");
        game.EmulatorConfig ??= new EmulatorConfig();
        Games.Add(game);
        SelectedGame = game;
        SaveData();
    }

    public void SaveData()
    {
        Directory.CreateDirectory(dataRootPath);
        var data = new PersistedApplicationData
        {
            ActiveProfileId = SelectedProfile?.Id,
            Profiles = Profiles.ToList(),
            Games = Games.ToList()
        };
        string temp = dataFilePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(data, JsonOptions));
        File.Move(temp, dataFilePath, true);
    }

    private void LoadOrCreateData()
    {
        try
        {
            if (!File.Exists(dataFilePath))
            {
                RunFirstRunSetup();
                return;
            }

            var data = JsonSerializer.Deserialize<PersistedApplicationData>(File.ReadAllText(dataFilePath), JsonOptions);
            if (data is null || data.Profiles.Count == 0)
            {
                RunFirstRunSetup();
                return;
            }

            foreach (var profile in data.Profiles)
            {
                if (profile.Id == Guid.Empty) profile.Id = Guid.NewGuid();
                if (string.IsNullOrWhiteSpace(profile.StorageRootPath))
                    profile.StorageRootPath = Path.Combine(dataRootPath, "Profiles", profile.Id.ToString());
                Directory.CreateDirectory(profile.StorageRootPath);
                Profiles.Add(profile);
            }

            foreach (var game in data.Games)
            {
                game.EmulatorConfig ??= new EmulatorConfig();
                Games.Add(game);
            }

            SelectedProfile = Profiles.FirstOrDefault(p => p.Id == data.ActiveProfileId) ?? Profiles.FirstOrDefault();
            SelectedGame = Games.FirstOrDefault();
        }
        catch
        {
            Profiles.Clear();
            Games.Clear();
            RunFirstRunSetup();
        }
    }

    private void RunFirstRunSetup()
    {
        var wizard = new FirstRunWizardDialog();
        if (wizard.ShowDialog() == true && wizard.CreatedProfile != null)
        {
            var profile = wizard.CreatedProfile;
            profile.StorageRootPath = Path.Combine(dataRootPath, "Profiles", profile.Id.ToString());
            Directory.CreateDirectory(profile.StorageRootPath);
            Profiles.Add(profile);
            SelectedProfile = profile;

            var settings = SettingsService.Instance.LoadSettings();
            settings.GlobalDefaultSaveMode = wizard.SelectedSaveMode;
            settings.MaxBackupsPerGame = wizard.BackupRetention;
            SettingsService.Instance.SaveSettings(settings);

            SaveData();
        }
        else
        {
            var fallback = CreateDefaultProfile();
            SelectedProfile = fallback;
            SaveData();
        }
    }

    private UserProfile CreateDefaultProfile()
    {
        var p = new UserProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "Primary Account",
            StorageRootPath = Path.Combine(dataRootPath, "Profiles", Guid.NewGuid().ToString())
        };
        Directory.CreateDirectory(p.StorageRootPath);
        Profiles.Add(p);
        return p;
    }

    private void AddProfile()
    {
        var d = new AddProfileDialog { Owner = Application.Current?.MainWindow };
        if (d.ShowDialog() == true && d.CreatedProfile != null) AddProfileFromDialog(d.CreatedProfile);
    }

    private void AddGame()
    {
        var d = new AddGameDialog { Owner = Application.Current?.MainWindow };
        if (d.ShowDialog() == true && d.CreatedGame != null) AddGameFromDialog(d.CreatedGame);
    }

    private void SwitchProfileForSelectedGame()
    {
        if (SelectedProfile is not null && SelectedGame is not null)
            saveManager.SwitchActiveProfileForGame(SelectedProfile, SelectedGame, SelectedGame.DefaultSaveMode);
    }

    private void OpenBackups()
    {
        if (SelectedProfile is not null && SelectedGame is not null)
            new BackupManagerDialog(SelectedProfile, SelectedGame, backupManager) { Owner = Application.Current?.MainWindow }.ShowDialog();
    }

    private void CreateZipBackup()
    {
        if (SelectedProfile is not null && SelectedGame is not null)
        {
            string path = SelectedGame.EmulatorConfig?.SaveRootPath ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(path)) backupManager.CreateBackup(SelectedProfile, SelectedGame, path);
        }
    }

    private void OpenAppStorage()
    {
        Directory.CreateDirectory(dataRootPath);
        Process.Start(new ProcessStartInfo(dataRootPath) { UseShellExecute = true });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) { this.execute = execute; this.canExecute = canExecute; }
        public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => execute(parameter);
        public event EventHandler? CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }
    }
}
