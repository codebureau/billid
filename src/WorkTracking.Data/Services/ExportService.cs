using System.Text;
using System.Text.Json;
using WorkTracking.Core.Models;
using WorkTracking.Core.Services;
using WorkTracking.Data.Repositories.Interfaces;

namespace WorkTracking.Data.Services;

public class ExportService(ISettingRepository settingRepository) : IExportService
{
    private const string SettingKey = "export_definition";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public async Task<ExportDefinition> LoadDefinitionAsync()
    {
        var json = await settingRepository.GetAsync(SettingKey);
        if (string.IsNullOrWhiteSpace(json))
            return new ExportDefinition();

        return JsonSerializer.Deserialize<ExportDefinition>(json, JsonOptions) ?? new ExportDefinition();
    }

    public async Task SaveDefinitionAsync(ExportDefinition definition)
    {
        var json = JsonSerializer.Serialize(definition, JsonOptions);
        await settingRepository.SetAsync(SettingKey, json);
    }

    public Task ExportToCsvAsync(
        IEnumerable<WorkEntry> entries,
        IReadOnlyDictionary<int, Client> clientsById,
        IReadOnlyDictionary<int, string> categoriesById,
        ExportDefinition definition,
        string filePath)
    {
        var sb = new StringBuilder();

        // Header
        var headers = BuildHeaders(definition);
        sb.AppendLine(string.Join(",", headers));

        // Rows
        foreach (var entry in entries)
        {
            clientsById.TryGetValue(entry.ClientId, out var client);
            var row = BuildRow(entry, client, categoriesById, definition);
            sb.AppendLine(string.Join(",", row));
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        return Task.CompletedTask;
    }

    private static IEnumerable<string> BuildHeaders(ExportDefinition d)
    {
        if (d.IncludeWorkEntryId)       yield return "Entry ID";
        if (d.IncludeDate)              yield return "Date";
        if (d.IncludeDescription)       yield return "Description";
        if (d.IncludeHours)             yield return "Hours";
        if (d.IncludeWorkCategory)      yield return "Work Category";
        if (d.IncludeInvoicedFlag)      yield return "Invoiced";
        if (d.IncludeInvoiceId)         yield return "Invoice ID";
        if (d.IncludeNotesMarkdown)     yield return "Notes";
        if (d.IncludeClientId)          yield return "Client ID";
        if (d.IncludeClientName)        yield return "Client Name";
        if (d.IncludeClientCompanyName) yield return "Company Name";
        if (d.IncludeClientEmail)       yield return "Client Email";
        if (d.IncludeClientPhone)       yield return "Client Phone";
        if (d.IncludeClientHourlyRate)  yield return "Hourly Rate";
        if (d.IncludeClientAbn)         yield return "ABN";
    }

    private static IEnumerable<string> BuildRow(
        WorkEntry entry,
        Client? client,
        IReadOnlyDictionary<int, string> categoriesById,
        ExportDefinition d)
    {
        if (d.IncludeWorkEntryId)       yield return CsvEscape(entry.Id.ToString());
        if (d.IncludeDate)              yield return CsvEscape(entry.Date.ToString("yyyy-MM-dd"));
        if (d.IncludeDescription)       yield return CsvEscape(entry.Description);
        if (d.IncludeHours)             yield return CsvEscape(entry.Hours.ToString("0.##"));
        if (d.IncludeWorkCategory)
        {
            var cat = entry.WorkCategoryId.HasValue && categoriesById.TryGetValue(entry.WorkCategoryId.Value, out var n) ? n : string.Empty;
            yield return CsvEscape(cat);
        }
        if (d.IncludeInvoicedFlag)      yield return entry.InvoicedFlag ? "Yes" : "No";
        if (d.IncludeInvoiceId)         yield return CsvEscape(entry.InvoiceId?.ToString() ?? string.Empty);
        if (d.IncludeNotesMarkdown)     yield return CsvEscape(entry.NotesMarkdown ?? string.Empty);
        if (d.IncludeClientId)          yield return CsvEscape(client?.Id.ToString() ?? string.Empty);
        if (d.IncludeClientName)        yield return CsvEscape(client?.Name ?? string.Empty);
        if (d.IncludeClientCompanyName) yield return CsvEscape(client?.CompanyName ?? string.Empty);
        if (d.IncludeClientEmail)       yield return CsvEscape(client?.Email ?? string.Empty);
        if (d.IncludeClientPhone)       yield return CsvEscape(client?.Phone ?? string.Empty);
        if (d.IncludeClientHourlyRate)  yield return CsvEscape(client?.HourlyRate.ToString("0.##") ?? string.Empty);
        if (d.IncludeClientAbn)         yield return CsvEscape(client?.Abn ?? string.Empty);
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
