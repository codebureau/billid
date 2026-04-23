using System.Collections.ObjectModel;
using System.Windows.Input;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Commands;

namespace WorkTracking.UI.ViewModels;

public class InvoicesViewModel(
    IInvoiceRepository invoiceRepository,
    IWorkEntryRepository workEntryRepository) : ViewModelBase
{
    private int _clientId;
    private ObservableCollection<InvoiceRowViewModel> _invoices = [];
    private InvoiceRowViewModel? _selectedInvoice;
    private bool _isLoading;

    public ObservableCollection<InvoiceRowViewModel> Invoices
    {
        get => _invoices;
        private set => SetField(ref _invoices, value);
    }

    public InvoiceRowViewModel? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (SetField(ref _selectedInvoice, value) && value is not null)
                _ = LoadInvoiceDetailAsync(value);
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public bool HasInvoices => Invoices.Count > 0;

    public ICommand OpenPdfCommand => new RelayCommand(
        param =>
        {
            if (param is string path && !string.IsNullOrEmpty(path))
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        },
        param => param is string path && !string.IsNullOrEmpty(path));

    public async Task LoadAsync(int clientId)
    {
        _clientId = clientId;
        IsLoading = true;
        try
        {
            var invoices = await invoiceRepository.GetByClientAsync(clientId);
            Invoices = [.. invoices.Select(i => new InvoiceRowViewModel(i))];
            OnPropertyChanged(nameof(HasInvoices));
            SelectedInvoice = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadInvoiceDetailAsync(InvoiceRowViewModel row)
    {
        var lines = await invoiceRepository.GetLinesAsync(row.Id);
        var entries = await workEntryRepository.GetByInvoiceIdAsync(row.Id);
        row.Lines = lines;
        row.LinkedEntries = entries;
    }
}
