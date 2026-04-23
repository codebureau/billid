using System.Collections.ObjectModel;
using System.Windows.Input;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Commands;

namespace WorkTracking.UI.ViewModels;

public class ClientListViewModel(IClientRepository clientRepository) : ViewModelBase
{
    private ObservableCollection<Client> _clients = [];
    private Client? _selectedClient;
    private string _searchText = string.Empty;

    public ObservableCollection<Client> Clients
    {
        get => _clients;
        private set => SetField(ref _clients, value);
    }

    public Client? SelectedClient
    {
        get => _selectedClient;
        set => SetField(ref _selectedClient, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
                ApplyFilter();
        }
    }

    public ICommand LoadClientsCommand => new RelayCommand(async _ => await LoadAsync());

    private List<Client> _allClients = [];

    public async Task LoadAsync()
    {
        _allClients = [.. await clientRepository.GetAllAsync()];
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allClients
            : _allClients.Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

        Clients = [.. filtered];
    }
}
