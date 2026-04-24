using WorkTracking.Core.Models;

namespace WorkTracking.Data.Repositories.Interfaces;

public interface IWorkCategoryRepository
{
    Task<IReadOnlyList<WorkCategory>> GetAllAsync();
    Task<IReadOnlyList<WorkCategory>> GetByClientAsync(int clientId);
    Task<WorkCategory?> GetByIdAsync(int id);
    Task<WorkCategory> AddAsync(WorkCategory category);
    Task UpdateAsync(WorkCategory category);
    Task DeleteAsync(int id);
    Task EnableForClientAsync(int clientId, int workCategoryId);
    Task DisableForClientAsync(int clientId, int workCategoryId);
}
