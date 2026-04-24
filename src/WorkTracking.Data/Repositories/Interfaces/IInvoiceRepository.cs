using WorkTracking.Core.Models;

namespace WorkTracking.Data.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<IReadOnlyList<Invoice>> GetByClientAsync(int clientId);
    Task<Invoice?> GetByIdAsync(int id);
    Task<IReadOnlyList<InvoiceLine>> GetLinesAsync(int invoiceId);
    Task<Invoice> AddWithLinesAsync(Invoice invoice, IEnumerable<InvoiceLine> lines);
    Task UpdateAsync(Invoice invoice);
    Task DeleteAsync(int id);
}
