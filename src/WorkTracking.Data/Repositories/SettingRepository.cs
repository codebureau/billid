using Microsoft.Data.Sqlite;
using WorkTracking.Core.Models;
using WorkTracking.Data.Database;
using WorkTracking.Data.Repositories.Interfaces;

namespace WorkTracking.Data.Repositories;

public class SettingRepository(IDatabaseConnectionFactory connectionFactory) : ISettingRepository
{
    public async Task<string?> GetAsync(string key)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM setting WHERE key = $key";
        command.Parameters.AddWithValue("$key", key);

        var result = await command.ExecuteScalarAsync();
        return result is DBNull or null ? null : (string)result;
    }

    public async Task SetAsync(string key, string? value)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR REPLACE INTO setting (key, value) VALUES ($key, $value)";
        command.Parameters.AddWithValue("$key", key);
        command.Parameters.AddWithValue("$value", (object?)value ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<Setting>> GetAllAsync()
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT key, value FROM setting ORDER BY key";

        var settings = new List<Setting>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            settings.Add(new Setting
            {
                Key = reader.GetString(0),
                Value = reader.IsDBNull(1) ? null : reader.GetString(1),
            });
        }

        return settings;
    }
}
