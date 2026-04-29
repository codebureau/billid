using System.Diagnostics;
using System.Windows.Input;
using WorkTracking.Core.Models;
using WorkTracking.UI.Commands;

namespace WorkTracking.UI.ViewModels;

public class MainWindowViewModel(
    ClientListViewModel clientListViewModel,
    ClientDetailViewModel clientDetailViewModel,
    HomeViewModel homeViewModel,
    AppSettingsViewModel appSettingsViewModel) : ViewModelBase
{
    public ClientListViewModel  ClientList    { get; } = clientListViewModel;
    public ClientDetailViewModel ClientDetail { get; } = clientDetailViewModel;
    public HomeViewModel         Home         { get; } = homeViewModel;
    public AppSettingsViewModel  AppSettings  { get; } = appSettingsViewModel;

#if DEBUG
    public string Title => "billid [DEV]";
#else
    public string Title => "billid";
#endif

    private bool _showSettings;

    public bool ShowHome         => ClientList.SelectedClient is null && !_showSettings;
    public bool ShowSettings     => _showSettings;
    public bool ShowClientDetail => ClientList.SelectedClient is not null;

    public event EventHandler? ShowAboutRequested;

    public ICommand ShowAboutCommand  => new RelayCommand(_ => ShowAboutRequested?.Invoke(this, EventArgs.Empty));
    public ICommand OpenHelpCommand   => new RelayCommand(_ => Process.Start(new ProcessStartInfo("https://codebureau.github.io/billid/") { UseShellExecute = true }));
    public ICommand GoHomeCommand     => new RelayCommand(_ => NavigateHome());
    public ICommand GoSettingsCommand => new RelayCommand(_ => NavigateSettings());

    public async Task InitializeAsync()
    {
        await ClientList.LoadAsync();
        await Home.LoadAsync();
        await AppSettings.LoadAsync();

        ClientList.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ClientListViewModel.SelectedClient))
                OnSelectedClientChanged(ClientList.SelectedClient);
        };

        ClientDetail.Settings.ClientUpdated += async (_, updatedClient) =>
        {
            await ClientList.LoadAsync();
            await Home.LoadAsync();
            ClientList.SelectedClient = updatedClient;
        };
    }

    private void NavigateHome()
    {
        _showSettings = false;
        ClientList.SelectedClient = null;
        NotifyPanelState();
    }

    private void NavigateSettings()
    {
        _showSettings = true;
        ClientList.SelectedClient = null;
        NotifyPanelState();
    }

    private void OnSelectedClientChanged(Client? client)
    {
        if (client is null)
        {
            ClientDetail.Clear();
            _ = Home.LoadAsync();
        }
        else
        {
            _showSettings = false;
            ClientDetail.LoadClient(client);
        }
        NotifyPanelState();
    }

    private void NotifyPanelState()
    {
        OnPropertyChanged(nameof(ShowHome));
        OnPropertyChanged(nameof(ShowSettings));
        OnPropertyChanged(nameof(ShowClientDetail));
    }
}
