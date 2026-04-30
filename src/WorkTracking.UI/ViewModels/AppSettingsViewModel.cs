using System.Windows.Input;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Commands;
using WorkTracking.UI.Services;

namespace WorkTracking.UI.ViewModels;

public class AppSettingsViewModel(
    IThemeService themeService,
    ISettingRepository settingRepository) : ViewModelBase
{
    private const string ThemeSettingKey = "ui_theme";
    private const string ShowDeactivatedClientsKey = "show_deactivated_clients";

    private AppTheme _selectedTheme = themeService.CurrentTheme;
    private bool _showDeactivatedClients;

    public AppTheme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetField(ref _selectedTheme, value))
                PreviewTheme(value);
        }
    }

    public bool ShowDeactivatedClients
    {
        get => _showDeactivatedClients;
        set => SetField(ref _showDeactivatedClients, value);
    }

    public IReadOnlyList<AppTheme> AvailableThemes { get; } = [AppTheme.Light, AppTheme.Dark];

    public ICommand SaveCommand => new RelayCommand(async _ => await SaveAsync());

    public async Task LoadAsync()
    {
        var stored = await settingRepository.GetAsync(ThemeSettingKey);
        if (Enum.TryParse<AppTheme>(stored, out var theme))
            _selectedTheme = theme;
        else
            _selectedTheme = AppTheme.Light;

        var showDeactivated = await settingRepository.GetAsync(ShowDeactivatedClientsKey);
        _showDeactivatedClients = showDeactivated == "true";

        OnPropertyChanged(nameof(SelectedTheme));
        OnPropertyChanged(nameof(ShowDeactivatedClients));
        themeService.Apply(_selectedTheme);
    }

    private void PreviewTheme(AppTheme theme) => themeService.Apply(theme);

    private async Task SaveAsync()
    {
        themeService.Apply(_selectedTheme);
        await settingRepository.SetAsync(ThemeSettingKey, _selectedTheme.ToString());
        await settingRepository.SetAsync(ShowDeactivatedClientsKey, _showDeactivatedClients ? "true" : "false");
    }
}
