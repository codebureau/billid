using System.Collections.ObjectModel;
using System.Windows.Input;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Commands;
using WorkTracking.UI.Services;

namespace WorkTracking.UI.ViewModels;

public class ClientListViewModel(
    IClientRepository clientRepository,
    IWorkEntryRepository workEntryRepository,
    IDialogService dialogService) : ViewModelBase
{
    private ObservableCollection<ClientRowViewModel> _clients = [];
    private ClientRowViewModel? _selectedRow;
    private string _searchText = string.Empty;

    public ObservableCollection<ClientRowViewModel> Clients
    {
        get => _clients;
        private set
        {
            SetField(ref _clients, value);
            OnPropertyChanged(nameof(HasClients));
        }
    }

    public bool HasClients => _clients.Count > 0;

    public ClientRowViewModel? SelectedRow
    {
        get => _selectedRow;
        set
        {
            if (SetField(ref _selectedRow, value))
                OnPropertyChanged(nameof(SelectedClient));
        }
    }

    public Client? SelectedClient
    {
        get => _selectedRow?.Client;
        set => SelectedRow = value is null ? null
            : _allClients.FirstOrDefault(r => r.Client.Id == value.Id)
              ?? new ClientRowViewModel(value, 0);
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
        SelectedClient = _allClients.FirstOrDefault(r => r.Client.Id == added.Id)?.Client;
    });

    public ICommand DeleteClientCommand => new RelayCommand(
        async _ =>
        {
            if (SelectedClient is null) return;
            if (!dialogService.Confirm($"Delete client '{SelectedClient.Name}'? This cannot be undone.", "Delete Client"))
                return;
            await clientRepository.DeleteAsync(SelectedClient.Id);
            await LoadAsync();
        },
        _ => SelectedClient is not null);

    private List<ClientRowViewModel> _allClients = [];

    public async Task LoadAsync()
    {
        var clients = await clientRepository.GetAllAsync();
        var hoursByClient = await workEntryRepository.GetUninvoicedHoursByClientAsync();

        _allClients = clients
            .Select(c => new ClientRowViewModel(c, hoursByClient.GetValueOrDefault(c.Id)))
            .ToList();

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allClients
            : _allClients.Where(r => r.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

        Clients = [.. filtered];
    }
}
