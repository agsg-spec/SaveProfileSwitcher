using System; using System.Windows; using System.Windows.Controls; using SaveProfileSwitcher.App.Models;
namespace SaveProfileSwitcher.App.Views;
public partial class AddGameDialog : Window
{
    public GameConfig CreatedGame { get; private set; } = new();
    public AddGameDialog() { InitializeComponent(); }
    private void RequiredField_TextChanged(object sender, TextChangedEventArgs e) => CreateGameButton.IsEnabled = !string.IsNullOrWhiteSpace(GameTitleTextBox.Text) && !string.IsNullOrWhiteSpace(TitleIdTextBox.Text) && !string.IsNullOrWhiteSpace(SaveDirectoryTextBox.Text);
    private void CreateGame_Click(object sender, RoutedEventArgs e)
    {
        string platform = PlatformComboBox.SelectedItem is ComboBoxItem item && item.Content is string text ? text : "PC";
        CreatedGame = new GameConfig { GameId = Guid.NewGuid().ToString("N"), GameName = GameTitleTextBox.Text.Trim(), DefaultSaveMode = SaveModeComboBox.SelectedIndex switch { 1 => SaveMode.MoveAndSwap, 2 => SaveMode.CopyAndOverwrite, _ => SaveMode.SymlinkOrJunction }, EmulatorConfig = new EmulatorConfig { PlatformName = platform, EmulatorType = platform switch { "RPCS3" => EmulatorType.RPCS3, "Xenia" => EmulatorType.Xenia, "Cemu" => EmulatorType.Cemu, "PCSX2" => EmulatorType.PCSX2, "Dolphin" => EmulatorType.Dolphin, "DuckStation" => EmulatorType.DuckStation, "Ryujinx" => EmulatorType.Ryujinx, _ => EmulatorType.Unknown }, TitleId = TitleIdTextBox.Text.Trim(), SaveRootPath = SaveDirectoryTextBox.Text.Trim(), ExecutablePath = ExecutablePathTextBox.Text.Trim() } };
        DialogResult = true;
    }
}
