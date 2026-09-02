using System.Windows;
using SaveProfileSwitcher.App.ViewModels;

namespace SaveProfileSwitcher.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void AddProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddProfileDialog { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            // Optionally refresh profiles list here
        }
    }

    private void AddGame_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Open AddGameDialog
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Open SettingsDialog
    }

    private void AppStorage_Click(object sender, RoutedEventArgs e)
    {
        var root = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "SaveProfileSwitcher");
        System.IO.Directory.CreateDirectory(root);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", "\"" + root + "\"") { UseShellExecute = true });
    }
}
