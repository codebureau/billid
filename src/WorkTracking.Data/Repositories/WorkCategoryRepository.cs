using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using WorkTracking.Core.Models;
using WorkTracking.Data.Database;
using WorkTracking.Data.Repositories.Interfaces;

namespace WorkTracking.Data.Repositories;

public class WorkCategoryRepository(IDatabaseConnectionFactory connectionFactory, ILogger<WorkCategoryRepository> logger) : IWorkCategoryRepository
{
    public async Task<IReadOnlyList<WorkCategory>> GetAllAsync()
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM work_category ORDER BY name";

        var categories = new List<WorkCategory>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            categories.Add(MapCategory(reader));

        return categories;
    }

    public async Task<IReadOnlyList<WorkCategory>> GetByClientAsync(int clientId)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT wc.* FROM work_category wc
            INNER JOIN client_work_category cwc ON cwc.work_category_id = wc.id
            WHERE cwc.client_id = $clientId
            ORDER BY wc.name
            """;
        command.Parameters.AddWithValue("$clientId", clientId);

        var categories = new List<WorkCategory>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            categories.Add(MapCategory(reader));

        return categories;
    }

    public async Task<WorkCategory?> GetByIdAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM work_category WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCategory(reader) : null;
    }

    public async Task<WorkCategory> AddAsync(WorkCategory category)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO work_category (name, description) VALUES ($name, $description);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$name", category.Name);
        command.Parameters.AddWithValue("$description", (object?)category.Description ?? DBNull.Value);

        category.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
        logger.LogInformation("Added work category {CategoryId} '{Name}'", category.Id, category.Name);
        return category;
    }

    public async Task UpdateAsync(WorkCategory category)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE work_category SET name = $name, description = $description WHERE id = $id";
        command.Parameters.AddWithValue("$name", category.Name);
        command.Parameters.AddWithValue("$description", (object?)category.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$id", category.Id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM work_category WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task EnableForClientAsync(int clientId, int workCategoryId)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT OR IGNORE INTO client_work_category (client_id, work_category_id)
            VALUES ($clientId, $workCategoryId)
            """;
        command.Parameters.AddWithValue("$clientId", clientId);
        command.Parameters.AddWithValue("$workCategoryId", workCategoryId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DisableForClientAsync(int clientId, int workCategoryId)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM client_work_category WHERE client_id = $clientId AND work_category_id = $workCategoryId";
        command.Parameters.AddWithValue("$clientId", clientId);
        command.Parameters.AddWithValue("$workCategoryId", workCategoryId);

        await command.ExecuteNonQueryAsync();
    }

    private static WorkCategory MapCategory(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        Name = reader.GetString(reader.GetOrdinal("name")),
        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
    };
}
