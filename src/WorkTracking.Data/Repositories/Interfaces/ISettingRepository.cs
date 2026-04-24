using WorkTracking.Core.Models;

namespace WorkTracking.Data.Repositories.Interfaces;

public interface ISettingRepository
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string? value);
    Task<IReadOnlyList<Setting>> GetAllAsync();
}
