using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SaveProfileSwitcher.App.Models;
using SaveProfileSwitcher.App.Services;
using SaveProfileSwitcher.App.ViewModels;
namespace SaveProfileSwitcher.App.Views;
public partial class MainWindow : Window
{
    public MainWindow() { InitializeComponent(); }
    private void AddProfile_Click(object sender, RoutedEventArgs e) { var dialog = new AddProfileDialog { Owner = this }; if (dialog.ShowDialog() == true && DataContext is MainViewModel vm) vm.AddProfileFromDialog(dialog.CreatedProfile); }
    private void AddGame_Click(object sender, RoutedEventArgs e) { var dialog = new AddGameDialog { Owner = this }; if (dialog.ShowDialog() == true && DataContext is MainViewModel vm) vm.AddGameFromDialog(dialog.CreatedGame); }
    private void Settings_Click(object sender, RoutedEventArgs e) { new SettingsDialog { Owner = this }.ShowDialog(); }
    private void EditGame_Click(object sender, RoutedEventArgs e) { if (sender is Button button && button.DataContext is GameConfig game && DataContext is MainViewModel vm) { if (new EditGameDialog(game) { Owner = this }.ShowDialog() == true) { vm.SaveData(); GamesListBox.Items.Refresh(); } } }
    private void QuickSwitch_Click(object sender, RoutedEventArgs e) { if (sender is Button button && button.DataContext is GameConfig game && button.Tag is ComboBox selector && selector.SelectedItem is UserProfile profile) { try { new SaveManagerService().SwitchActiveProfileForGame(profile, game, game.DefaultSaveMode); } catch (Exception ex) { MessageBox.Show(this, ex.Message, "Quick Switch Failed", MessageBoxButton.OK, MessageBoxImage.Error); } } }
    private void Launch_Click(object sender, RoutedEventArgs e) { if (sender is Button button && button.DataContext is GameConfig game && !string.IsNullOrWhiteSpace(game.EmulatorConfig?.ExecutablePath)) { try { Process.Start(new ProcessStartInfo(game.EmulatorConfig.ExecutablePath) { UseShellExecute = true }); } catch (Exception ex) { MessageBox.Show(this, ex.Message, "Launch Failed", MessageBoxButton.OK, MessageBoxImage.Error); } } }
    private void Backups_Click(object sender, RoutedEventArgs e) { if (sender is Button button && button.DataContext is GameConfig game && DataContext is MainViewModel vm && vm.SelectedProfile is UserProfile profile) new BackupManagerDialog(profile, game, new BackupManagerService()) { Owner = this }.ShowDialog(); }
    private void TransferSave_Click(object sender, RoutedEventArgs e) { if (DataContext is MainViewModel vm && vm.SelectedGame is not null && vm.SelectedProfile is not null) vm.SwitchProfileForSelectedGameCommand.Execute(null); }
    private void CreateZipBackup_Click(object sender, RoutedEventArgs e) { if (DataContext is MainViewModel vm) vm.CreateZipBackupCommand.Execute(null); }
    private void AppStorage_Click(object sender, RoutedEventArgs e) { if (DataContext is MainViewModel vm) vm.OpenAppStorageCommand.Execute(null); }
}
