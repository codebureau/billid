using WorkTracking.Core.Models;

namespace WorkTracking.Data.Repositories.Interfaces;

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(int id);
    Task<Client> AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task DeleteAsync(int id);
}
