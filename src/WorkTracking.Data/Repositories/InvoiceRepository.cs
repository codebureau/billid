using Microsoft.Data.Sqlite;
using WorkTracking.Core.Models;
using WorkTracking.Data.Database;
using WorkTracking.Data.Helpers;
using WorkTracking.Data.Repositories.Interfaces;

namespace WorkTracking.Data.Repositories;

public class InvoiceRepository(IDatabaseConnectionFactory connectionFactory) : IInvoiceRepository
{
    public async Task<IReadOnlyList<Invoice>> GetByClientAsync(int clientId)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM invoice WHERE client_id = $clientId ORDER BY invoice_date DESC";
        command.Parameters.AddWithValue("$clientId", clientId);

        var invoices = new List<Invoice>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            invoices.Add(MapInvoice(reader));

        return invoices;
    }

    public async Task<Invoice?> GetByIdAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM invoice WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapInvoice(reader) : null;
    }

    public async Task<IReadOnlyList<InvoiceLine>> GetLinesAsync(int invoiceId)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM invoice_line WHERE invoice_id = $invoiceId";
        command.Parameters.AddWithValue("$invoiceId", invoiceId);

        var lines = new List<InvoiceLine>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lines.Add(MapInvoiceLine(reader));

        return lines;
    }

    public async Task<Invoice> AddWithLinesAsync(Invoice invoice, IEnumerable<InvoiceLine> lines)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await using var invoiceCmd = connection.CreateCommand();
            invoiceCmd.Transaction = (SqliteTransaction)transaction;
            var now = DateConversion.ToIso8601(DateTime.UtcNow);
            invoiceCmd.CommandText = """
                INSERT INTO invoice (client_id, invoice_number, invoice_date, total_amount, pdf_path, created_at, updated_at)
                VALUES ($clientId, $invoiceNumber, $invoiceDate, $totalAmount, $pdfPath, $createdAt, $updatedAt);
                SELECT last_insert_rowid();
                """;
            invoiceCmd.Parameters.AddWithValue("$clientId", invoice.ClientId);
            invoiceCmd.Parameters.AddWithValue("$invoiceNumber", invoice.InvoiceNumber);
            invoiceCmd.Parameters.AddWithValue("$invoiceDate", DateConversion.ToIso8601(invoice.InvoiceDate));
            invoiceCmd.Parameters.AddWithValue("$totalAmount", (double)invoice.TotalAmount);
            invoiceCmd.Parameters.AddWithValue("$pdfPath", (object?)invoice.PdfPath ?? DBNull.Value);
            invoiceCmd.Parameters.AddWithValue("$createdAt", now);
            invoiceCmd.Parameters.AddWithValue("$updatedAt", now);

            invoice.Id = Convert.ToInt32(await invoiceCmd.ExecuteScalarAsync());

            foreach (var line in lines)
            {
                await using var lineCmd = connection.CreateCommand();
                lineCmd.Transaction = (SqliteTransaction)transaction;
                lineCmd.CommandText = """
                    INSERT INTO invoice_line (invoice_id, work_category_id, hours, rate, amount, description)
                    VALUES ($invoiceId, $workCategoryId, $hours, $rate, $amount, $description);
                    SELECT last_insert_rowid();
                    """;
                lineCmd.Parameters.AddWithValue("$invoiceId", invoice.Id);
                lineCmd.Parameters.AddWithValue("$workCategoryId",
                    line.WorkCategoryId.HasValue ? (object)line.WorkCategoryId.Value : DBNull.Value);
                lineCmd.Parameters.AddWithValue("$hours", (double)line.Hours);
                lineCmd.Parameters.AddWithValue("$rate", (double)line.Rate);
                lineCmd.Parameters.AddWithValue("$amount", (double)line.Amount);
                lineCmd.Parameters.AddWithValue("$description", (object?)line.Description ?? DBNull.Value);

                line.Id = Convert.ToInt32(await lineCmd.ExecuteScalarAsync());
                line.InvoiceId = invoice.Id;
            }

            await transaction.CommitAsync();
            return invoice;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Invoice invoice)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE invoice SET
                invoice_number = $invoiceNumber, invoice_date = $invoiceDate,
                total_amount = $totalAmount, pdf_path = $pdfPath, updated_at = $updatedAt
            WHERE id = $id
            """;
        command.Parameters.AddWithValue("$invoiceNumber", invoice.InvoiceNumber);
        command.Parameters.AddWithValue("$invoiceDate", DateConversion.ToIso8601(invoice.InvoiceDate));
        command.Parameters.AddWithValue("$totalAmount", (double)invoice.TotalAmount);
        command.Parameters.AddWithValue("$pdfPath", (object?)invoice.PdfPath ?? DBNull.Value);
        command.Parameters.AddWithValue("$updatedAt", DateConversion.ToIso8601(DateTime.UtcNow));
        command.Parameters.AddWithValue("$id", invoice.Id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM invoice WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        await command.ExecuteNonQueryAsync();
    }

    private static Invoice MapInvoice(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        ClientId = reader.GetInt32(reader.GetOrdinal("client_id")),
        InvoiceNumber = reader.GetString(reader.GetOrdinal("invoice_number")),
        InvoiceDate = DateConversion.ToDateOnly(reader.GetString(reader.GetOrdinal("invoice_date"))),
        TotalAmount = (decimal)reader.GetDouble(reader.GetOrdinal("total_amount")),
        PdfPath = reader.IsDBNull(reader.GetOrdinal("pdf_path")) ? null : reader.GetString(reader.GetOrdinal("pdf_path")),
        CreatedAt = DateConversion.ToDateTime(reader.GetString(reader.GetOrdinal("created_at"))),
        UpdatedAt = DateConversion.ToDateTime(reader.GetString(reader.GetOrdinal("updated_at"))),
    };

    private static InvoiceLine MapInvoiceLine(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        InvoiceId = reader.GetInt32(reader.GetOrdinal("invoice_id")),
        WorkCategoryId = reader.IsDBNull(reader.GetOrdinal("work_category_id")) ? null : reader.GetInt32(reader.GetOrdinal("work_category_id")),
        Hours = (decimal)reader.GetDouble(reader.GetOrdinal("hours")),
        Rate = (decimal)reader.GetDouble(reader.GetOrdinal("rate")),
        Amount = (decimal)reader.GetDouble(reader.GetOrdinal("amount")),
        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
    };
}
