using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using WorkTracking.Data.Database;
using WorkTracking.UI.DependencyInjection;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var services = new ServiceCollection();

        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddWorkTrackingServices();

        _serviceProvider = services.BuildServiceProvider();

        var initializer = _serviceProvider.GetRequiredService<SchemaInitializer>();
        await initializer.InitializeAsync();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var vm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.DataContext = vm;
        mainWindow.Show();

        await vm.InitializeAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

