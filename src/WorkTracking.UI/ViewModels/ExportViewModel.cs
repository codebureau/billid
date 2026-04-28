using System.Windows.Input;
using WorkTracking.Core.Models;
using WorkTracking.Core.Services;
using WorkTracking.UI.Commands;

namespace WorkTracking.UI.ViewModels;

public class ExportViewModel(IExportService exportService) : ViewModelBase
{
    private ExportDefinition _definition = new();

    // Work entry field toggles
    public bool IncludeWorkEntryId
    {
        get => _definition.IncludeWorkEntryId;
        set { _definition.IncludeWorkEntryId = value; OnPropertyChanged(); }
    }
    public bool IncludeDate
    {
        get => _definition.IncludeDate;
        set { _definition.IncludeDate = value; OnPropertyChanged(); }
    }
    public bool IncludeDescription
    {
        get => _definition.IncludeDescription;
        set { _definition.IncludeDescription = value; OnPropertyChanged(); }
    }
    public bool IncludeHours
    {
        get => _definition.IncludeHours;
        set { _definition.IncludeHours = value; OnPropertyChanged(); }
    }
    public bool IncludeWorkCategory
    {
        get => _definition.IncludeWorkCategory;
        set { _definition.IncludeWorkCategory = value; OnPropertyChanged(); }
    }
    public bool IncludeInvoicedFlag
    {
        get => _definition.IncludeInvoicedFlag;
        set { _definition.IncludeInvoicedFlag = value; OnPropertyChanged(); }
    }
    public bool IncludeInvoiceId
    {
        get => _definition.IncludeInvoiceId;
        set { _definition.IncludeInvoiceId = value; OnPropertyChanged(); }
    }
    public bool IncludeNotesMarkdown
    {
        get => _definition.IncludeNotesMarkdown;
        set { _definition.IncludeNotesMarkdown = value; OnPropertyChanged(); }
    }

    // Client field toggles
    public bool IncludeClientId
    {
        get => _definition.IncludeClientId;
        set { _definition.IncludeClientId = value; OnPropertyChanged(); }
    }
    public bool IncludeClientName
    {
        get => _definition.IncludeClientName;
        set { _definition.IncludeClientName = value; OnPropertyChanged(); }
    }
    public bool IncludeClientCompanyName
    {
        get => _definition.IncludeClientCompanyName;
        set { _definition.IncludeClientCompanyName = value; OnPropertyChanged(); }
    }
    public bool IncludeClientEmail
    {
        get => _definition.IncludeClientEmail;
        set { _definition.IncludeClientEmail = value; OnPropertyChanged(); }
    }
    public bool IncludeClientPhone
    {
        get => _definition.IncludeClientPhone;
        set { _definition.IncludeClientPhone = value; OnPropertyChanged(); }
    }
    public bool IncludeClientHourlyRate
    {
        get => _definition.IncludeClientHourlyRate;
        set { _definition.IncludeClientHourlyRate = value; OnPropertyChanged(); }
    }
    public bool IncludeClientAbn
    {
        get => _definition.IncludeClientAbn;
        set { _definition.IncludeClientAbn = value; OnPropertyChanged(); }
    }

    public ICommand ResetToDefaultsCommand => new RelayCommand(_ => ApplyDefinition(new ExportDefinition()));

    public event EventHandler<bool>? CloseRequested;

    public ICommand ExportCommand => new RelayCommand(async _ =>
    {
        await exportService.SaveDefinitionAsync(_definition);
        CloseRequested?.Invoke(this, true);
    });

    public ICommand CancelCommand => new RelayCommand(_ => CloseRequested?.Invoke(this, false));

    public async Task LoadAsync()
    {
        var definition = await exportService.LoadDefinitionAsync();
        ApplyDefinition(definition);
    }

    public ExportDefinition GetDefinition() => _definition;

    private void ApplyDefinition(ExportDefinition d)
    {
        _definition = d;
        OnPropertyChanged(nameof(IncludeWorkEntryId));
        OnPropertyChanged(nameof(IncludeDate));
        OnPropertyChanged(nameof(IncludeDescription));
        OnPropertyChanged(nameof(IncludeHours));
        OnPropertyChanged(nameof(IncludeWorkCategory));
        OnPropertyChanged(nameof(IncludeInvoicedFlag));
        OnPropertyChanged(nameof(IncludeInvoiceId));
        OnPropertyChanged(nameof(IncludeNotesMarkdown));
        OnPropertyChanged(nameof(IncludeClientId));
        OnPropertyChanged(nameof(IncludeClientName));
        OnPropertyChanged(nameof(IncludeClientCompanyName));
        OnPropertyChanged(nameof(IncludeClientEmail));
        OnPropertyChanged(nameof(IncludeClientPhone));
        OnPropertyChanged(nameof(IncludeClientHourlyRate));
        OnPropertyChanged(nameof(IncludeClientAbn));
    }
}
