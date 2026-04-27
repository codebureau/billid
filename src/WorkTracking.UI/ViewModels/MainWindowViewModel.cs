using System.Windows.Input;
using WorkTracking.Core.Models;
using WorkTracking.UI.Commands;

namespace WorkTracking.UI.ViewModels;

public class MainWindowViewModel(
    ClientListViewModel clientListViewModel,
    ClientDetailViewModel clientDetailViewModel,
    HomeViewModel homeViewModel) : ViewModelBase
{
    public ClientListViewModel ClientList { get; } = clientListViewModel;
    public ClientDetailViewModel ClientDetail { get; } = clientDetailViewModel;
    public HomeViewModel Home { get; } = homeViewModel;

    public bool ShowHome => ClientList.SelectedClient is null;

    public event EventHandler? ShowAboutRequested;

    public ICommand ShowAboutCommand => new RelayCommand(_ => ShowAboutRequested?.Invoke(this, EventArgs.Empty));
    public ICommand GoHomeCommand    => new RelayCommand(_ => ClientList.SelectedClient = null);

    public async Task InitializeAsync()
    {
        await ClientList.LoadAsync();
        await Home.LoadAsync();

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

    private void OnSelectedClientChanged(Client? client)
    {
        if (client is null)
        {
            ClientDetail.Clear();
            _ = Home.LoadAsync();
        }
        else
        {
            ClientDetail.LoadClient(client);
        }
        OnPropertyChanged(nameof(ShowHome));
    }
}
