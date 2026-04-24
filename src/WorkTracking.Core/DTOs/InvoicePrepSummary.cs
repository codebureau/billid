using WorkTracking.Core.Enums;

namespace WorkTracking.Core.DTOs;

public class InvoicePrepSummary
{
    public int ClientId { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal? InvoiceCapAmount { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceCapStatus CapStatus { get; set; }
    public IReadOnlyList<InvoicePrepCategoryLine> LinesByCategory { get; set; } = [];
}

public class InvoicePrepCategoryLine
{
    public int? WorkCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal Amount { get; set; }
}
