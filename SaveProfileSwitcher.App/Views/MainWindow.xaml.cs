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
}
