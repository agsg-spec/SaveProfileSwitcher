using System.Windows;

namespace SaveProfileSwitcher.App.Views;

public partial class AddProfileDialog : Window
{
    public AddProfileDialog()
    {
        InitializeComponent();
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        var name = DisplayNameTextBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("Please enter a display name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // In a real app, create the profile here and add to the collection.
        DialogResult = true;
    }
}
