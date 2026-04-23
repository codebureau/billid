using WorkTracking.Core.Models;

namespace WorkTracking.UI.ViewModels;

public class InvoiceRowViewModel(Invoice invoice) : ViewModelBase
{
    private IReadOnlyList<InvoiceLine> _lines = [];
    private IReadOnlyList<WorkEntry> _linkedEntries = [];

    public Invoice Invoice { get; } = invoice;
    public int Id => Invoice.Id;
    public string InvoiceNumber => Invoice.InvoiceNumber;
    public DateOnly InvoiceDate => Invoice.InvoiceDate;
    public decimal TotalAmount => Invoice.TotalAmount;
    public string? PdfPath => Invoice.PdfPath;
    public bool HasPdf => !string.IsNullOrEmpty(Invoice.PdfPath);

    public IReadOnlyList<InvoiceLine> Lines
    {
        get => _lines;
        set => SetField(ref _lines, value);
    }

    public IReadOnlyList<WorkEntry> LinkedEntries
    {
        get => _linkedEntries;
        set
        {
            SetField(ref _linkedEntries, value);
            OnPropertyChanged(nameof(EntryCount));
        }
    }

    public int EntryCount => _linkedEntries.Count;
}
