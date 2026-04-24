namespace WorkTracking.Core.Models;

public class WorkEntry
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public int? WorkCategoryId { get; set; }
    public string? NotesMarkdown { get; set; }
    public bool InvoicedFlag { get; set; }
    public int? InvoiceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
