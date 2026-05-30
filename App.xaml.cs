using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WaterReminder;

public partial class App : System.Windows.Application
{
    private Mutex? _singleInstanceMutex;
    private AppController? _controller;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _singleInstanceMutex = new Mutex(true, @"Local\WaterReminder.Singleton", out var createdNew);
        if (!createdNew)
        {
            Shutdown();
            return;
        }

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        _controller = new AppController(new LogService());
        _controller.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DispatcherUnhandledException -= OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

        _controller?.Dispose();
        _controller = null;

        if (_singleInstanceMutex is not null)
        {
            _singleInstanceMutex.ReleaseMutex();
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
        }

        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _controller?.LogService.Write("UI", e.Exception);
        e.Handled = true;
        var text = LocalizationService.CurrentText;
        System.Windows.MessageBox.Show(
            text.ErrorMessage,
            text.ErrorTitle,
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error);
        Shutdown(-1);
    }

    private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _controller?.LogService.Write("APPDOMAIN", exception);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _controller?.LogService.Write("TASK", e.Exception);
        e.SetObserved();
    }
}
