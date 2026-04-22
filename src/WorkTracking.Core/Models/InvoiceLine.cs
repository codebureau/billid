namespace WorkTracking.Core.Models;

public class InvoiceLine
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int? WorkCategoryId { get; set; }
    public decimal Hours { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
