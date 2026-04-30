namespace WorkTracking.Core.Models;

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? Abn { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal? InvoiceCapAmount { get; set; }
    public string? InvoiceCapBehavior { get; set; }
    public int? InvoiceFrequencyDays { get; set; }
    public DateOnly? LastInvoiceDate { get; set; }
    public DateOnly? NextInvoiceDueDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
