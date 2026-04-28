using Microsoft.Extensions.DependencyInjection;
using WorkTracking.Core.Services;
using WorkTracking.Data.Database;
using WorkTracking.Data.Repositories;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.Data.Services;
using WorkTracking.UI.Services;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.UI.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkTrackingServices(this IServiceCollection services)
    {
        // Infrastructure
        services.AddSingleton<IDatabaseConnectionFactory>(_ =>
            new DatabaseConnectionFactory(DatabaseConnectionFactory.GetDefaultConnectionString()));
        services.AddSingleton<SchemaInitializer>();

        // Repositories (Scoped)
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IWorkEntryRepository, WorkEntryRepository>();
        services.AddScoped<IWorkCategoryRepository, WorkCategoryRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<IExportService, ExportService>();

        // Services (Singleton)
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IThemeService, ThemeService>();

        // ViewModels (Transient)
        services.AddTransient<ExportViewModel>();
        services.AddTransient<TimesheetViewModel>();
        services.AddTransient<InvoicesViewModel>();
        services.AddTransient<SummaryViewModel>();
        services.AddTransient<ClientSettingsViewModel>();
        services.AddTransient<ClientListViewModel>();
        services.AddTransient<ClientDetailViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<AppSettingsViewModel>();
        services.AddTransient<MainWindowViewModel>();

        // Main window (Singleton — one instance)
        services.AddSingleton<MainWindow>();

        return services;
    }
}
