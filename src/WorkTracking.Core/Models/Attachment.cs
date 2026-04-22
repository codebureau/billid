namespace WorkTracking.Core.Models;

public class Attachment
{
    public int Id { get; set; }
    public int WorkEntryId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public string? FilePath { get; set; }
    public DateTime CreatedAt { get; set; }
}
