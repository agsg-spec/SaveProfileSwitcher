using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SaveProfileSwitcher.App.Models;

namespace SaveProfileSwitcher.App.Views;

public partial class FirstRunWizardDialog : Window
{
    public UserProfile? CreatedProfile { get; private set; }
    public SaveMode SelectedSaveMode { get; private set; } = SaveMode.SymlinkOrJunction;
    public int BackupRetention { get; private set; } = 3;

    public FirstRunWizardDialog()
    {
        InitializeComponent();
        ValidationChanged(this, null!);
    }

    private void ValidationChanged(object sender, TextChangedEventArgs e)
    {
        if (GetStartedButton != null)
        {
            GetStartedButton.IsEnabled = !string.IsNullOrWhiteSpace(DisplayNameTextBox.Text);
        }
    }

    private void RetentionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (RetentionValueTextBlock != null)
        {
            RetentionValueTextBlock.Text = ((int)e.NewValue).ToString();
        }
    }

    private void BrowseAvatar_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Avatar Image",
            Filter = "Image Files (*.png;*.jpg;*.jpeg;*.webp;*.ico)|*.png;*.jpg;*.jpeg;*.webp;*.ico|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == true)
        {
            AvatarPathTextBox.Text = dialog.FileName;
        }
    }

    private void GetStarted_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DisplayNameTextBox.Text))
        {
            return;
        }

        CreatedProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = DisplayNameTextBox.Text.Trim(),
            AvatarPath = AvatarPathTextBox.Text.Trim()
        };

        SelectedSaveMode = SaveModeComboBox.SelectedIndex switch
        {
            1 => SaveMode.MoveAndSwap,
            2 => SaveMode.CopyAndOverwrite,
            _ => SaveMode.SymlinkOrJunction
        };

        BackupRetention = (int)RetentionSlider.Value;
        DialogResult = true;
    }
}