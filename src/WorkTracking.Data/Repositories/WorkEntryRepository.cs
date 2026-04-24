using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using WorkTracking.Core.Models;
using WorkTracking.Data.Database;
using WorkTracking.Data.Helpers;
using WorkTracking.Data.Repositories.Interfaces;

namespace WorkTracking.Data.Repositories;

public class WorkEntryRepository(IDatabaseConnectionFactory connectionFactory, ILogger<WorkEntryRepository> logger) : IWorkEntryRepository
{
    public async Task<IReadOnlyList<WorkEntry>> GetByClientAsync(int clientId)
        => await GetFilteredAsync(clientId);

    public async Task<IReadOnlyList<WorkEntry>> GetByInvoiceIdAsync(int invoiceId)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM work_entry WHERE invoice_id = $invoiceId ORDER BY date DESC";
        command.Parameters.AddWithValue("$invoiceId", invoiceId);

        var entries = new List<WorkEntry>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            entries.Add(MapWorkEntry(reader));

        return entries;
    }

    public async Task<IReadOnlyList<WorkEntry>> GetFilteredAsync(
        int clientId,
        DateOnly? from = null,
        DateOnly? to = null,
        bool? invoiced = null,
        int? workCategoryId = null)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        var sql = "SELECT * FROM work_entry WHERE client_id = $clientId";
        command.Parameters.AddWithValue("$clientId", clientId);

        if (from.HasValue)
        {
            sql += " AND date >= $from";
            command.Parameters.AddWithValue("$from", DateConversion.ToIso8601(from.Value));
        }
        if (to.HasValue)
        {
            sql += " AND date <= $to";
            command.Parameters.AddWithValue("$to", DateConversion.ToIso8601(to.Value));
        }
        if (invoiced.HasValue)
        {
            sql += " AND invoiced_flag = $invoiced";
            command.Parameters.AddWithValue("$invoiced", invoiced.Value ? 1 : 0);
        }
        if (workCategoryId.HasValue)
        {
            sql += " AND work_category_id = $categoryId";
            command.Parameters.AddWithValue("$categoryId", workCategoryId.Value);
        }

        sql += " ORDER BY date DESC";
        command.CommandText = sql;

        var entries = new List<WorkEntry>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            entries.Add(MapWorkEntry(reader));

        return entries;
    }

    public async Task<WorkEntry?> GetByIdAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM work_entry WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapWorkEntry(reader) : null;
    }

    public async Task<WorkEntry> AddAsync(WorkEntry entry)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO work_entry (client_id, date, description, hours, work_category_id,
                notes_markdown, invoiced_flag, invoice_id, created_at, updated_at)
            VALUES ($clientId, $date, $description, $hours, $workCategoryId,
                $notesMarkdown, $invoicedFlag, $invoiceId, $createdAt, $updatedAt);
            SELECT last_insert_rowid();
            """;
        BindWorkEntryParameters(command, entry);

        entry.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
        logger.LogInformation("Added work entry {EntryId} for client {ClientId}", entry.Id, entry.ClientId);
        return entry;
    }

    public async Task UpdateAsync(WorkEntry entry)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE work_entry SET
                client_id = $clientId, date = $date, description = $description,
                hours = $hours, work_category_id = $workCategoryId,
                notes_markdown = $notesMarkdown, invoiced_flag = $invoicedFlag,
                invoice_id = $invoiceId, updated_at = $updatedAt
            WHERE id = $id
            """;
        BindWorkEntryParameters(command, entry);
        command.Parameters.AddWithValue("$id", entry.Id);

        await command.ExecuteNonQueryAsync();
        logger.LogInformation("Updated work entry {EntryId}", entry.Id);
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM work_entry WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        await command.ExecuteNonQueryAsync();
        logger.LogInformation("Deleted work entry {EntryId}", id);
    }

    public async Task MarkInvoicedAsync(IEnumerable<int> ids, int invoiceId)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var placeholders = string.Join(",", idList.Select((_, i) => $"$id{i}"));

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            UPDATE work_entry
            SET invoiced_flag = 1, invoice_id = $invoiceId, updated_at = $updatedAt
            WHERE id IN ({placeholders})
            """;
        command.Parameters.AddWithValue("$invoiceId", invoiceId);
        command.Parameters.AddWithValue("$updatedAt", DateConversion.ToIso8601(DateTime.UtcNow));
        for (var i = 0; i < idList.Count; i++)
            command.Parameters.AddWithValue($"$id{i}", idList[i]);

        await command.ExecuteNonQueryAsync();
    }

    private static void BindWorkEntryParameters(SqliteCommand command, WorkEntry entry)
    {
        var now = DateConversion.ToIso8601(DateTime.UtcNow);
        command.Parameters.AddWithValue("$clientId", entry.ClientId);
        command.Parameters.AddWithValue("$date", DateConversion.ToIso8601(entry.Date));
        command.Parameters.AddWithValue("$description", entry.Description);
        command.Parameters.AddWithValue("$hours", (double)entry.Hours);
        command.Parameters.AddWithValue("$workCategoryId",
            entry.WorkCategoryId.HasValue ? (object)entry.WorkCategoryId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$notesMarkdown", (object?)entry.NotesMarkdown ?? DBNull.Value);
        command.Parameters.AddWithValue("$invoicedFlag", entry.InvoicedFlag ? 1 : 0);
        command.Parameters.AddWithValue("$invoiceId",
            entry.InvoiceId.HasValue ? (object)entry.InvoiceId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", now);
        command.Parameters.AddWithValue("$updatedAt", now);
    }

    private static WorkEntry MapWorkEntry(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        ClientId = reader.GetInt32(reader.GetOrdinal("client_id")),
        Date = DateConversion.ToDateOnly(reader.GetString(reader.GetOrdinal("date"))),
        Description = reader.GetString(reader.GetOrdinal("description")),
        Hours = (decimal)reader.GetDouble(reader.GetOrdinal("hours")),
        WorkCategoryId = reader.IsDBNull(reader.GetOrdinal("work_category_id")) ? null : reader.GetInt32(reader.GetOrdinal("work_category_id")),
        NotesMarkdown = reader.IsDBNull(reader.GetOrdinal("notes_markdown")) ? null : reader.GetString(reader.GetOrdinal("notes_markdown")),
        InvoicedFlag = reader.GetInt32(reader.GetOrdinal("invoiced_flag")) != 0,
        InvoiceId = reader.IsDBNull(reader.GetOrdinal("invoice_id")) ? null : reader.GetInt32(reader.GetOrdinal("invoice_id")),
        CreatedAt = DateConversion.ToDateTime(reader.GetString(reader.GetOrdinal("created_at"))),
        UpdatedAt = DateConversion.ToDateTime(reader.GetString(reader.GetOrdinal("updated_at"))),
    };
}
