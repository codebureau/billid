using System.Windows.Input;
using WorkTracking.Core.Enums;
using WorkTracking.Core.Services;
using WorkTracking.UI.Commands;

namespace WorkTracking.UI.ViewModels;

public class InvoicePrepViewModel : ViewModelBase
{
    private string _invoiceNumber = string.Empty;
    private DateTime _invoiceDate = DateTime.Today;
    private string? _pdfPath;
    private bool _confirmed;

    public InvoicePrepViewModel(
        IReadOnlyList<WorkEntryRowViewModel> selectedEntries,
        decimal hourlyRate,
        decimal? invoiceCapAmount)
    {
        SelectedEntries = selectedEntries;

        var uninvoiced = selectedEntries.Where(e => !e.IsInvoiced).ToList();
        TotalHours = uninvoiced.Sum(e => e.Hours);
        TotalAmount = TotalHours * hourlyRate;
        CapStatus = InvoiceCapCalculator.Calculate(invoiceCapAmount, TotalAmount);

        LinesByCategory = uninvoiced
            .GroupBy(e => new { CategoryName = string.IsNullOrEmpty(e.CategoryName) ? "Uncategorised" : e.CategoryName, e.Entry.WorkCategoryId })
            .Select(g => new InvoicePrepLine(g.Key.WorkCategoryId, g.Key.CategoryName, g.Sum(e => e.Hours), g.Sum(e => e.Hours) * hourlyRate))
            .OrderBy(l => l.CategoryName)
            .ToList();

        ConfirmCommand = new RelayCommand(
            _ => { _confirmed = true; CloseRequested?.Invoke(this, true); },
            _ => !string.IsNullOrWhiteSpace(_invoiceNumber));

        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, false));
    }

    public IReadOnlyList<WorkEntryRowViewModel> SelectedEntries { get; }
    public decimal TotalHours { get; }
    public decimal TotalAmount { get; }
    public InvoiceCapStatus CapStatus { get; }
    public bool IsOverCap => CapStatus == InvoiceCapStatus.OverCap;
    public IReadOnlyList<InvoicePrepLine> LinesByCategory { get; }

    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set => SetField(ref _invoiceNumber, value);
    }

    public DateTime InvoiceDate
    {
        get => _invoiceDate;
        set => SetField(ref _invoiceDate, value);
    }

    public string? PdfPath
    {
        get => _pdfPath;
        set => SetField(ref _pdfPath, value);
    }

    public bool Confirmed => _confirmed;

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler<bool>? CloseRequested;
}

public record InvoicePrepLine(int? WorkCategoryId, string CategoryName, decimal Hours, decimal Amount);
