using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using SaveProfileSwitcher.App.Models;

namespace SaveProfileSwitcher.App.Views;

public partial class AddProfileDialog : Window
{
    public UserProfile CreatedProfile { get; private set; } = new();

    public string DisplayName => DisplayNameTextBox.Text;

    public AddProfileDialog()
    {
        InitializeComponent();
    }

    private void DisplayNameTextBox_TextChanged(
        object sender,
        System.Windows.Controls.TextChangedEventArgs e)
    {
        CreateButton.IsEnabled = !string.IsNullOrWhiteSpace(DisplayNameTextBox.Text);
    }

    private void BrowseAvatar_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.webp;*.ico|All Files|*.*"
        };

        if (dialog.ShowDialog(this) == true)
        {
            AvatarPathTextBox.Text = dialog.FileName;
        }
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        Guid id = Guid.NewGuid();

        CreatedProfile = new UserProfile
        {
            Id = id,
            DisplayName = DisplayNameTextBox.Text.Trim(),
            AvatarPath = string.IsNullOrWhiteSpace(AvatarPathTextBox.Text)
                ? null
                : AvatarPathTextBox.Text.Trim(),
            StorageRootPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "SaveProfileSwitcher",
                "Profiles",
                id.ToString())
        };

        DialogResult = true;
    }
}
