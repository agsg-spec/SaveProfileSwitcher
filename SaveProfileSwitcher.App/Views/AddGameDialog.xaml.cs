using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SaveProfileSwitcher.App.Models;
namespace SaveProfileSwitcher.App.Views;
public partial class AddGameDialog : Window
{
    public GameConfig CreatedGame { get; private set; } = new();
    public AddGameDialog() { InitializeComponent(); }
    private void ValidationChanged(object sender, RoutedEventArgs e) { SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(GameNameTextBox.Text) && !string.IsNullOrWhiteSpace(GameIdTextBox.Text) && !string.IsNullOrWhiteSpace(SaveDirectoryTextBox.Text) && (string.IsNullOrWhiteSpace(ExecutablePathTextBox.Text) || File.Exists(ExecutablePathTextBox.Text)); }
    private void BrowseExecutable_Click(object sender, RoutedEventArgs e) { var d = new OpenFileDialog { Filter = "Executable files|*.exe|All files|*.*" }; if (d.ShowDialog(this) == true) { ExecutablePathTextBox.Text = d.FileName; ValidationChanged(this, null!); } }
    private void BrowseSaveDirectory_Click(object sender, RoutedEventArgs e) { var d = new OpenFolderDialog { Title = "Select live save folder" }; if (d.ShowDialog(this) == true) { SaveDirectoryTextBox.Text = d.FolderName; DetectionStatusTextBlock.Text = "Detected"; DetectionStatusTextBlock.Foreground = (System.Windows.Media.Brush)FindResource("SuccessBrush"); ValidationChanged(this, null!); } }
    private void OpenSaveDirectory_Click(object sender, RoutedEventArgs e) { if (Directory.Exists(SaveDirectoryTextBox.Text)) Process.Start(new ProcessStartInfo(SaveDirectoryTextBox.Text) { UseShellExecute = true }); }
    private void DetectSavePath_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrWhiteSpace(SaveDirectoryTextBox.Text)) { DetectionStatusTextBlock.Text = "Manual path preserved"; return; } DetectionStatusTextBlock.Text = "Not detected — select a path manually"; DetectionStatusTextBlock.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush"); }
    private void Save_Click(object sender, RoutedEventArgs e) { string platform = PlatformComboBox.SelectedItem is ComboBoxItem p && p.Content is string ps ? ps : "PC"; CreatedGame = new GameConfig { GameId = GameIdTextBox.Text.Trim(), GameName = GameNameTextBox.Text.Trim(), DefaultSaveMode = SaveModeComboBox.SelectedIndex switch { 1 => SaveMode.SymlinkOrJunction, 2 => SaveMode.MoveAndSwap, 3 => SaveMode.CopyAndOverwrite, _ => SaveMode.CopyAndOverwrite }, EmulatorConfig = new EmulatorConfig { PlatformName = platform, TitleId = GameIdTextBox.Text.Trim(), SaveRootPath = SaveDirectoryTextBox.Text.Trim(), ExecutablePath = ExecutablePathTextBox.Text.Trim(), EmulatorType = EmulatorType.Unknown } }; DialogResult = true; }
}
