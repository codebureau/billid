using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WorkTracking.Data.Database;
using WorkTracking.UI.DependencyInjection;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public App()
    {
        var logFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Billable", "logs");
        Directory.CreateDirectory(logFolder);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: Path.Combine(logFolder, "billable-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        DispatcherUnhandledException += (_, e) =>
        {
            Log.Error(e.Exception, "Unhandled UI thread exception");
            e.Handled = true;
            ShowErrorDialog(e.Exception);
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Log.Fatal(ex, "Unhandled AppDomain exception (terminating: {IsTerminating})", e.IsTerminating);
            Log.CloseAndFlush();
            if (e.IsTerminating)
                ShowErrorDialog(ex);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };

        EventManager.RegisterClassHandler(
            typeof(TextBox),
            UIElement.GotKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler((s, _) => ((TextBox)s).SelectAll()));
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            var splash = new SplashWindow();
            splash.Show();

            var services = new ServiceCollection();
            services.AddLogging(b => b
                .ClearProviders()
                .AddSerilog(dispose: false)
                .SetMinimumLevel(LogLevel.Information));
            services.AddWorkTrackingServices();
            _serviceProvider = services.BuildServiceProvider();

            var initTask = _serviceProvider.GetRequiredService<SchemaInitializer>().InitializeAsync();
            await Task.WhenAll(initTask, Task.Delay(2000));

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var vm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            mainWindow.DataContext = vm;
            mainWindow.Show();

            splash.Close();

            await vm.InitializeAsync();

            Log.Information("Billable started successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error during startup");
            Log.CloseAndFlush();
            ShowErrorDialog(ex);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Billable shutting down");
        Log.CloseAndFlush();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void ShowErrorDialog(Exception? ex)
    {
        var logFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Billable", "logs");
        MessageBox.Show(
            $"An unexpected error occurred and Billable needs to close.\n\n" +
            $"{ex?.Message}\n\n" +
            $"A log file has been written to:\n{logFolder}",
            "Unexpected Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}

