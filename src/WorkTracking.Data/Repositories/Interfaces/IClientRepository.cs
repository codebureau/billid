using WorkTracking.Core.Models;

namespace WorkTracking.Data.Repositories.Interfaces;

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> GetAllAsync(bool includeInactive = false);
    Task<Client?> GetByIdAsync(int id);
    Task<Client> AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task SetActiveAsync(int id, bool active);
    Task ReorderAsync(IReadOnlyList<int> orderedIds);
    Task DeleteAsync(int id);
}
