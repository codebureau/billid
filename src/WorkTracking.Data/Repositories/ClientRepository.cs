using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using WorkTracking.Core.Models;
using WorkTracking.Data.Database;
using WorkTracking.Data.Helpers;
using WorkTracking.Data.Repositories.Interfaces;

namespace WorkTracking.Data.Repositories;

public class ClientRepository(IDatabaseConnectionFactory connectionFactory, ILogger<ClientRepository> logger) : IClientRepository
{
    public async Task<IReadOnlyList<Client>> GetAllAsync()
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM client ORDER BY name";

        var clients = new List<Client>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            clients.Add(MapClient(reader));

        return clients;
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM client WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapClient(reader) : null;
    }

    public async Task<Client> AddAsync(Client client)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO client (name, contact_name, company_name, address, abn, email, phone,
                hourly_rate, invoice_cap_amount, invoice_cap_behavior, invoice_frequency_days,
                last_invoice_date, next_invoice_due_date, created_at, updated_at)
            VALUES ($name, $contactName, $companyName, $address, $abn, $email, $phone,
                $hourlyRate, $invoiceCapAmount, $invoiceCapBehavior, $invoiceFrequencyDays,
                $lastInvoiceDate, $nextInvoiceDueDate, $createdAt, $updatedAt);
            SELECT last_insert_rowid();
            """;
        BindClientParameters(command, client);

        client.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
        logger.LogInformation("Added client {ClientId} '{Name}'", client.Id, client.Name);
        return client;
    }

    public async Task UpdateAsync(Client client)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE client SET
                name = $name, contact_name = $contactName, company_name = $companyName,
                address = $address, abn = $abn, email = $email, phone = $phone,
                hourly_rate = $hourlyRate, invoice_cap_amount = $invoiceCapAmount,
                invoice_cap_behavior = $invoiceCapBehavior,
                invoice_frequency_days = $invoiceFrequencyDays,
                last_invoice_date = $lastInvoiceDate,
                next_invoice_due_date = $nextInvoiceDueDate,
                updated_at = $updatedAt
            WHERE id = $id
            """;
        BindClientParameters(command, client);
        command.Parameters.AddWithValue("$id", client.Id);

        await command.ExecuteNonQueryAsync();
        logger.LogInformation("Updated client {ClientId} '{Name}'", client.Id, client.Name);
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM client WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        await command.ExecuteNonQueryAsync();
        logger.LogInformation("Deleted client {ClientId}", id);
    }

    private static void BindClientParameters(SqliteCommand command, Client client)
    {
        var now = DateConversion.ToIso8601(DateTime.UtcNow);
        command.Parameters.AddWithValue("$name", client.Name);
        command.Parameters.AddWithValue("$contactName", (object?)client.ContactName ?? DBNull.Value);
        command.Parameters.AddWithValue("$companyName", (object?)client.CompanyName ?? DBNull.Value);
        command.Parameters.AddWithValue("$address", (object?)client.Address ?? DBNull.Value);
        command.Parameters.AddWithValue("$abn", (object?)client.Abn ?? DBNull.Value);
        command.Parameters.AddWithValue("$email", (object?)client.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("$phone", (object?)client.Phone ?? DBNull.Value);
        command.Parameters.AddWithValue("$hourlyRate", (double)client.HourlyRate);
        command.Parameters.AddWithValue("$invoiceCapAmount",
            client.InvoiceCapAmount.HasValue ? (object)(double)client.InvoiceCapAmount.Value : DBNull.Value);
        command.Parameters.AddWithValue("$invoiceCapBehavior", (object?)client.InvoiceCapBehavior ?? DBNull.Value);
        command.Parameters.AddWithValue("$invoiceFrequencyDays",
            client.InvoiceFrequencyDays.HasValue ? (object)client.InvoiceFrequencyDays.Value : DBNull.Value);
        command.Parameters.AddWithValue("$lastInvoiceDate",
            (object?)DateConversion.ToIso8601Nullable(client.LastInvoiceDate) ?? DBNull.Value);
        command.Parameters.AddWithValue("$nextInvoiceDueDate",
            (object?)DateConversion.ToIso8601Nullable(client.NextInvoiceDueDate) ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", now);
        command.Parameters.AddWithValue("$updatedAt", now);
    }

    private static Client MapClient(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        Name = reader.GetString(reader.GetOrdinal("name")),
        ContactName = reader.IsDBNull(reader.GetOrdinal("contact_name")) ? null : reader.GetString(reader.GetOrdinal("contact_name")),
        CompanyName = reader.IsDBNull(reader.GetOrdinal("company_name")) ? null : reader.GetString(reader.GetOrdinal("company_name")),
        Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString(reader.GetOrdinal("address")),
        Abn = reader.IsDBNull(reader.GetOrdinal("abn")) ? null : reader.GetString(reader.GetOrdinal("abn")),
        Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
        Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
        HourlyRate = (decimal)reader.GetDouble(reader.GetOrdinal("hourly_rate")),
        InvoiceCapAmount = reader.IsDBNull(reader.GetOrdinal("invoice_cap_amount")) ? null : (decimal)reader.GetDouble(reader.GetOrdinal("invoice_cap_amount")),
        InvoiceCapBehavior = reader.IsDBNull(reader.GetOrdinal("invoice_cap_behavior")) ? null : reader.GetString(reader.GetOrdinal("invoice_cap_behavior")),
        InvoiceFrequencyDays = reader.IsDBNull(reader.GetOrdinal("invoice_frequency_days")) ? null : reader.GetInt32(reader.GetOrdinal("invoice_frequency_days")),
        LastInvoiceDate = reader.IsDBNull(reader.GetOrdinal("last_invoice_date")) ? null : DateConversion.ToDateOnly(reader.GetString(reader.GetOrdinal("last_invoice_date"))),
        NextInvoiceDueDate = reader.IsDBNull(reader.GetOrdinal("next_invoice_due_date")) ? null : DateConversion.ToDateOnly(reader.GetString(reader.GetOrdinal("next_invoice_due_date"))),
        CreatedAt = DateConversion.ToDateTime(reader.GetString(reader.GetOrdinal("created_at"))),
        UpdatedAt = DateConversion.ToDateTime(reader.GetString(reader.GetOrdinal("updated_at"))),
    };
}
