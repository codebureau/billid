using System.Collections.ObjectModel;
using System.Windows.Input;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Commands;
using WorkTracking.UI.Services;

namespace WorkTracking.UI.ViewModels;

public class ClientListViewModel(IClientRepository clientRepository, IDialogService dialogService) : ViewModelBase
{
    private ObservableCollection<Client> _clients = [];
    private Client? _selectedClient;
    private string _searchText = string.Empty;

    public ObservableCollection<Client> Clients
    {
        get => _clients;
        private set
        {
            SetField(ref _clients, value);
            OnPropertyChanged(nameof(HasClients));
        }
    }

    public bool HasClients => _clients.Count > 0;

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

    public ICommand AddClientCommand => new RelayCommand(async _ =>
    {
        var vm = new AddClientViewModel();
        if (!dialogService.ShowAddClientDialog(vm)) return;

        var client = new Client
        {
            Name = vm.Name,
            HourlyRate = vm.HourlyRate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var added = await clientRepository.AddAsync(client);
        await LoadAsync();
        SelectedClient = _allClients.FirstOrDefault(c => c.Id == added.Id);
    });

    public ICommand DeleteClientCommand => new RelayCommand(
        async _ =>
        {
            if (_selectedClient is null) return;
            if (!dialogService.Confirm($"Delete client '{_selectedClient.Name}'? This cannot be undone.", "Delete Client"))
                return;
            await clientRepository.DeleteAsync(_selectedClient.Id);
            await LoadAsync();
        },
        _ => _selectedClient is not null);

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
