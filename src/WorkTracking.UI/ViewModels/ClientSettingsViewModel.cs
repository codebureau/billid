using System.Collections.ObjectModel;
using System.Windows.Input;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Commands;
using WorkTracking.UI.Services;
using CommandManager = System.Windows.Input.CommandManager;

namespace WorkTracking.UI.ViewModels;

public class ClientSettingsViewModel(
    IClientRepository clientRepository,
    IWorkCategoryRepository workCategoryRepository,
    IDialogService dialogService) : ViewModelBase
{
    private Client? _client;
    private bool _isDirty;
    private bool _isSaving;
    private string _newCategoryName = string.Empty;

    // --- editable fields ---
    private string _name = string.Empty;
    private string? _contactName;
    private string? _companyName;
    private string? _address;
    private string? _abn;
    private string? _email;
    private string? _phone;
    private decimal _hourlyRate;
    private decimal? _invoiceCapAmount;
    private string? _invoiceCapBehavior;
    private int? _invoiceFrequencyDays;

    public static IReadOnlyList<string> CapBehaviorOptions { get; } = ["warn", "block", "allow"];

    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (SetField(ref _isDirty, value))
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set => SetField(ref _isSaving, value);
    }

    public string Name
    {
        get => _name;
        set { if (SetField(ref _name, value)) IsDirty = true; }
    }

    public string? ContactName
    {
        get => _contactName;
        set { if (SetField(ref _contactName, value)) IsDirty = true; }
    }

    public string? CompanyName
    {
        get => _companyName;
        set { if (SetField(ref _companyName, value)) IsDirty = true; }
    }

    public string? Address
    {
        get => _address;
        set { if (SetField(ref _address, value)) IsDirty = true; }
    }

    public string? Abn
    {
        get => _abn;
        set { if (SetField(ref _abn, value)) IsDirty = true; }
    }

    public string? Email
    {
        get => _email;
        set { if (SetField(ref _email, value)) IsDirty = true; }
    }

    public string? Phone
    {
        get => _phone;
        set { if (SetField(ref _phone, value)) IsDirty = true; }
    }

    public decimal HourlyRate
    {
        get => _hourlyRate;
        set { if (SetField(ref _hourlyRate, value)) IsDirty = true; }
    }

    public decimal? InvoiceCapAmount
    {
        get => _invoiceCapAmount;
        set { if (SetField(ref _invoiceCapAmount, value)) IsDirty = true; }
    }

    public string? InvoiceCapBehavior
    {
        get => _invoiceCapBehavior;
        set { if (SetField(ref _invoiceCapBehavior, value)) IsDirty = true; }
    }

    public int? InvoiceFrequencyDays
    {
        get => _invoiceFrequencyDays;
        set { if (SetField(ref _invoiceFrequencyDays, value)) IsDirty = true; }
    }

    public ObservableCollection<CategoryToggleViewModel> Categories { get; } = [];

    public bool IsActive => _client?.IsActive ?? true;
    public bool IsReadOnly => !IsActive;

    public ICommand SaveCommand => new RelayCommand(
        async _ => await SaveAsync(),
        _ => IsDirty && !IsSaving && !string.IsNullOrWhiteSpace(_name) && IsActive);

    public ICommand DeactivateCommand => new RelayCommand(
        async _ =>
        {
            if (_client is null) return;
            if (!dialogService.Confirm($"Deactivate '{_client.Name}'? They will be hidden from the client list.", "Deactivate Client")) return;
            await clientRepository.SetActiveAsync(_client.Id, false);
            _client.IsActive = false;
            OnPropertyChanged(nameof(IsActive));
            OnPropertyChanged(nameof(IsReadOnly));
            CommandManager.InvalidateRequerySuggested();
            ClientUpdated?.Invoke(this, _client);
        },
        _ => IsActive);

    public ICommand ReactivateCommand => new RelayCommand(
        async _ =>
        {
            if (_client is null) return;
            await clientRepository.SetActiveAsync(_client.Id, true);
            _client.IsActive = true;
            OnPropertyChanged(nameof(IsActive));
            OnPropertyChanged(nameof(IsReadOnly));
            CommandManager.InvalidateRequerySuggested();
            ClientUpdated?.Invoke(this, _client);
        },
        _ => !IsActive);

    public ICommand DeleteClientCommand => new RelayCommand(
        async _ =>
        {
            if (_client is null) return;
            if (!dialogService.Confirm($"Delete client '{_client.Name}'? This cannot be undone.", "Delete Client")) return;
            await clientRepository.DeleteAsync(_client.Id);
            ClientDeleted?.Invoke(this, EventArgs.Empty);
        },
        _ => !IsActive);

    public string NewCategoryName
    {
        get => _newCategoryName;
        set => SetField(ref _newCategoryName, value);
    }

    public ICommand AddCategoryCommand => new RelayCommand(
        async _ =>
        {
            if (string.IsNullOrWhiteSpace(_newCategoryName) || _client is null) return;
            var category = await workCategoryRepository.AddAsync(new WorkCategory { Name = _newCategoryName.Trim() });
            await workCategoryRepository.EnableForClientAsync(_client.Id, category.Id);
            Categories.Add(new CategoryToggleViewModel(category, isEnabled: true));
            NewCategoryName = string.Empty;
        },
        _ => !string.IsNullOrWhiteSpace(_newCategoryName));

    public event EventHandler<Client>? ClientUpdated;
    public event EventHandler? ClientDeleted;

    public async Task LoadAsync(Client client)
    {
        _client = client;

        Name = client.Name;
        ContactName = client.ContactName;
        CompanyName = client.CompanyName;
        Address = client.Address;
        Abn = client.Abn;
        Email = client.Email;
        Phone = client.Phone;
        HourlyRate = client.HourlyRate;
        InvoiceCapAmount = client.InvoiceCapAmount;
        InvoiceCapBehavior = client.InvoiceCapBehavior;
        InvoiceFrequencyDays = client.InvoiceFrequencyDays;

        var allCategories = await workCategoryRepository.GetAllAsync();
        var enabledCategories = await workCategoryRepository.GetByClientAsync(client.Id);
        var enabledIds = enabledCategories.Select(c => c.Id).ToHashSet();

        Categories.Clear();
        foreach (var cat in allCategories.OrderBy(c => c.Name))
        {
            var toggle = new CategoryToggleViewModel(cat, enabledIds.Contains(cat.Id));
            toggle.PropertyChanged += (_, _) => IsDirty = true;
            Categories.Add(toggle);
        }

        IsDirty = false;
        OnPropertyChanged(nameof(IsActive));
        OnPropertyChanged(nameof(IsReadOnly));
        CommandManager.InvalidateRequerySuggested();
    }

    public async Task SaveAsync()
    {
        if (_client is null) return;

        IsSaving = true;
        try
        {
            _client.Name = Name;
            _client.ContactName = ContactName;
            _client.CompanyName = CompanyName;
            _client.Address = Address;
            _client.Abn = Abn;
            _client.Email = Email;
            _client.Phone = Phone;
            _client.HourlyRate = HourlyRate;
            _client.InvoiceCapAmount = InvoiceCapAmount;
            _client.InvoiceCapBehavior = InvoiceCapBehavior;
            _client.InvoiceFrequencyDays = InvoiceFrequencyDays;
            _client.UpdatedAt = DateTime.UtcNow;

            await clientRepository.UpdateAsync(_client);

            var enabledCategories = await workCategoryRepository.GetByClientAsync(_client.Id);
            var wasEnabled = enabledCategories.Select(c => c.Id).ToHashSet();
            var nowEnabled = Categories.Where(c => c.IsEnabled).Select(c => c.Id).ToHashSet();

            foreach (var id in nowEnabled.Except(wasEnabled))
                await workCategoryRepository.EnableForClientAsync(_client.Id, id);

            foreach (var id in wasEnabled.Except(nowEnabled))
                await workCategoryRepository.DisableForClientAsync(_client.Id, id);

            IsDirty = false;
            ClientUpdated?.Invoke(this, _client);
        }
        catch (Exception ex)
        {
            dialogService.ShowError($"Failed to save client settings: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }
}
