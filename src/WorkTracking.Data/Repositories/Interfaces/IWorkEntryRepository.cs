using WorkTracking.Core.Models;

namespace WorkTracking.Data.Repositories.Interfaces;

public interface IWorkEntryRepository
{
    Task<IReadOnlyList<WorkEntry>> GetByClientAsync(int clientId);
    Task<IReadOnlyList<WorkEntry>> GetByInvoiceIdAsync(int invoiceId);
    Task<IReadOnlyList<WorkEntry>> GetFilteredAsync(
        int clientId,
        DateOnly? from = null,
        DateOnly? to = null,
        bool? invoiced = null,
        int? workCategoryId = null);
    Task<WorkEntry?> GetByIdAsync(int id);
    Task<Dictionary<int, decimal>> GetUninvoicedHoursByClientAsync();
    Task<WorkEntry> AddAsync(WorkEntry entry);
    Task UpdateAsync(WorkEntry entry);
    Task DeleteAsync(int id);
    Task MarkInvoicedAsync(IEnumerable<int> ids, int invoiceId);
}
