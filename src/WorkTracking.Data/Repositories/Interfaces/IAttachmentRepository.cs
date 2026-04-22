using WorkTracking.Core.Models;

namespace WorkTracking.Data.Repositories.Interfaces;

public interface IAttachmentRepository
{
    Task<IReadOnlyList<Attachment>> GetByWorkEntryAsync(int workEntryId);
    Task<Attachment?> GetByIdAsync(int id);
    Task<Attachment> AddAsync(Attachment attachment);
    Task DeleteAsync(int id);
}
