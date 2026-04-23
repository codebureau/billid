using WorkTracking.Core.Models;

namespace WorkTracking.UI.ViewModels;

public class WorkEntryRowViewModel(WorkEntry entry, string? categoryName) : ViewModelBase
{
    private bool _isSelected;

    public WorkEntry Entry { get; } = entry;
    public int Id => Entry.Id;
    public DateOnly Date => Entry.Date;
    public string Description => Entry.Description;
    public decimal Hours => Entry.Hours;
    public string CategoryName { get; } = categoryName ?? string.Empty;
    public bool IsInvoiced => Entry.InvoicedFlag;
    public string InvoiceRef => Entry.InvoiceId.HasValue ? $"#{Entry.InvoiceId}" : string.Empty;
    public string? NotesMarkdown => Entry.NotesMarkdown;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }
}
