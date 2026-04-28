using WorkTracking.Core.Models;

namespace WorkTracking.Core.Services;

public interface IExportService
{
    Task<ExportDefinition> LoadDefinitionAsync();
    Task SaveDefinitionAsync(ExportDefinition definition);
    Task ExportToCsvAsync(
        IEnumerable<WorkEntry> entries,
        IReadOnlyDictionary<int, Client> clientsById,
        IReadOnlyDictionary<int, string> categoriesById,
        ExportDefinition definition,
        string filePath);
}
