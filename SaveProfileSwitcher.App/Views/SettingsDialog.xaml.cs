using System; using System.IO; using System.Windows; using Microsoft.Win32; using SaveProfileSwitcher.App.Models; using SaveProfileSwitcher.App.Services;
namespace SaveProfileSwitcher.App.Views;
public partial class SettingsDialog : Window
{
    public SettingsDialog() { InitializeComponent(); LoadSettings(); }
    private void LoadSettings() { Settings settings = SettingsService.Instance.LoadSettings(); EnableAutomaticBackupsCheckBox.IsChecked = settings.EnableAutomaticBackups; DefaultBackupLocationTextBox.Text = settings.DefaultBackupLocation; EnableLoggingCheckBox.IsChecked = settings.EnableLogging; DarkModeCheckBox.IsChecked = settings.DarkMode; }
    private void BrowseBackupLocation_Click(object sender, RoutedEventArgs e) { var dialog = new OpenFolderDialog { Title = "Select Default Backup Location" }; if (dialog.ShowDialog(this) == true) DefaultBackupLocationTextBox.Text = dialog.FolderName; }
    private void SaveSettings_Click(object sender, RoutedEventArgs e) { var settings = new Settings { EnableAutomaticBackups = EnableAutomaticBackupsCheckBox.IsChecked ?? false, DefaultBackupLocation = string.IsNullOrWhiteSpace(DefaultBackupLocationTextBox.Text) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SaveProfileSwitcher", "Backups") : DefaultBackupLocationTextBox.Text.Trim(), EnableLogging = EnableLoggingCheckBox.IsChecked ?? true, DarkMode = DarkModeCheckBox.IsChecked ?? false }; SettingsService.Instance.SaveSettings(settings); DialogResult = true; }
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
