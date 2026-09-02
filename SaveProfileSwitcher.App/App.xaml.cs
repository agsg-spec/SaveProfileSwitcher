using System;
using System.Threading.Tasks;
using System.Windows;
using SaveProfileSwitcher.App.Services;

namespace SaveProfileSwitcher.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        LoggerService.Instance.LogInfo("Application starting.");
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LoggerService.Instance.LogError("Dispatcher exception.", e.Exception);
        ShowError(e.Exception);
        e.Handled = true;
    }

    private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            LoggerService.Instance.LogError("Unhandled exception.", exception);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LoggerService.Instance.LogError("Unobserved task exception.", e.Exception);
        e.SetObserved();
    }

    private static void ShowError(Exception exception)
    {
        MessageBox.Show("An unexpected error occurred: " + exception.Message, "SaveProfileSwitcher", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
