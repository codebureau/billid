namespace WorkTracking.Core.Models;

/// <summary>
/// Defines which fields to include in a CSV export.
/// Persisted as JSON in the setting table under key "export_definition".
/// </summary>
public class ExportDefinition
{
    // Work entry fields
    public bool IncludeWorkEntryId       { get; set; } = false;
    public bool IncludeDate              { get; set; } = true;
    public bool IncludeDescription       { get; set; } = true;
    public bool IncludeHours             { get; set; } = true;
    public bool IncludeWorkCategory      { get; set; } = true;
    public bool IncludeInvoicedFlag      { get; set; } = true;
    public bool IncludeInvoiceId         { get; set; } = false;
    public bool IncludeNotesMarkdown     { get; set; } = false;

    // Client fields
    public bool IncludeClientId          { get; set; } = false;
    public bool IncludeClientName        { get; set; } = true;
    public bool IncludeClientCompanyName { get; set; } = true;
    public bool IncludeClientEmail       { get; set; } = false;
    public bool IncludeClientPhone       { get; set; } = false;
    public bool IncludeClientHourlyRate  { get; set; } = true;
    public bool IncludeClientAbn         { get; set; } = false;
}
