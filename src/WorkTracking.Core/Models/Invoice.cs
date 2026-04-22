namespace WorkTracking.Core.Models;

public class Invoice
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PdfPath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
