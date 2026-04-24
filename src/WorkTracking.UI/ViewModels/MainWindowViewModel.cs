using WorkTracking.Core.Models;

namespace WorkTracking.UI.ViewModels;

public class MainWindowViewModel(
    ClientListViewModel clientListViewModel,
    ClientDetailViewModel clientDetailViewModel) : ViewModelBase
{
    public ClientListViewModel ClientList { get; } = clientListViewModel;
    public ClientDetailViewModel ClientDetail { get; } = clientDetailViewModel;

    public async Task InitializeAsync()
    {
        await ClientList.LoadAsync();

        ClientList.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ClientListViewModel.SelectedClient))
                OnSelectedClientChanged(ClientList.SelectedClient);
        };
    }

    private void OnSelectedClientChanged(Client? client)
    {
        if (client is null)
            ClientDetail.Clear();
        else
            ClientDetail.LoadClient(client);
    }
}
