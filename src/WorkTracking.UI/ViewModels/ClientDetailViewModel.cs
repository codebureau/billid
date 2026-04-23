using WorkTracking.Core.Models;

namespace WorkTracking.UI.ViewModels;

public class ClientDetailViewModel(
    TimesheetViewModel timesheetViewModel,
    InvoicesViewModel invoicesViewModel,
    SummaryViewModel summaryViewModel,
    ClientSettingsViewModel clientSettingsViewModel) : ViewModelBase
{
    private Client? _client;
    private int _selectedTabIndex;

    public TimesheetViewModel Timesheet { get; } = timesheetViewModel;
    public InvoicesViewModel Invoices { get; } = invoicesViewModel;
    public SummaryViewModel Summary { get; } = summaryViewModel;
    public ClientSettingsViewModel Settings { get; } = clientSettingsViewModel;

    public Client? Client
    {
        get => _client;
        set => SetField(ref _client, value);
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetField(ref _selectedTabIndex, value);
    }

    public bool HasClient => _client is not null;

    public void LoadClient(Client client)
    {
        Client = client;
        OnPropertyChanged(nameof(HasClient));
        SelectedTabIndex = 0;
        _ = Timesheet.LoadAsync(client.Id, client.HourlyRate, client.InvoiceCapAmount);
        _ = Invoices.LoadAsync(client.Id);
        _ = Summary.LoadAsync(client);
        _ = Settings.LoadAsync(client);
        Timesheet.InvoiceCreated += (_, _) =>
        {
            _ = Invoices.LoadAsync(client.Id);
            _ = Summary.LoadAsync(client);
        };
        Settings.ClientUpdated += (_, updatedClient) =>
        {
            Client = updatedClient;
            OnPropertyChanged(nameof(HasClient));
        };
    }

    public void Clear()
    {
        Client = null;
        OnPropertyChanged(nameof(HasClient));
    }
}
