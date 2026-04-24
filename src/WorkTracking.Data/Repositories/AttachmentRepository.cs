using Microsoft.Data.Sqlite;
using WorkTracking.Core.Models;
using WorkTracking.Data.Database;
using WorkTracking.Data.Helpers;
using WorkTracking.Data.Repositories.Interfaces;

namespace WorkTracking.Data.Repositories;

public class AttachmentRepository(IDatabaseConnectionFactory connectionFactory) : IAttachmentRepository
{
    public async Task<IReadOnlyList<Attachment>> GetByWorkEntryAsync(int workEntryId)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM attachment WHERE work_entry_id = $workEntryId ORDER BY created_at";
        command.Parameters.AddWithValue("$workEntryId", workEntryId);

        var attachments = new List<Attachment>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            attachments.Add(MapAttachment(reader));

        return attachments;
    }

    public async Task<Attachment?> GetByIdAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM attachment WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapAttachment(reader) : null;
    }

    public async Task<Attachment> AddAsync(Attachment attachment)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO attachment (work_entry_id, filename, mime_type, file_path, created_at)
            VALUES ($workEntryId, $filename, $mimeType, $filePath, $createdAt);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$workEntryId", attachment.WorkEntryId);
        command.Parameters.AddWithValue("$filename", attachment.Filename);
        command.Parameters.AddWithValue("$mimeType", (object?)attachment.MimeType ?? DBNull.Value);
        command.Parameters.AddWithValue("$filePath", (object?)attachment.FilePath ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", DateConversion.ToIso8601(DateTime.UtcNow));

        attachment.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return attachment;
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM attachment WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        await command.ExecuteNonQueryAsync();
    }

    private static Attachment MapAttachment(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        WorkEntryId = reader.GetInt32(reader.GetOrdinal("work_entry_id")),
        Filename = reader.GetString(reader.GetOrdinal("filename")),
        MimeType = reader.IsDBNull(reader.GetOrdinal("mime_type")) ? null : reader.GetString(reader.GetOrdinal("mime_type")),
        FilePath = reader.IsDBNull(reader.GetOrdinal("file_path")) ? null : reader.GetString(reader.GetOrdinal("file_path")),
        CreatedAt = DateConversion.ToDateTime(reader.GetString(reader.GetOrdinal("created_at"))),
    };
}
