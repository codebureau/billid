using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WorkTracking.Data.Database;

namespace WorkTracking.Tests.Data;

public class SchemaInitializerTests
{
    [Fact]
    public async Task InitializeAsync_OnFreshDatabase_CreatesAllTables()
    {
        using var fixture = new SqliteTestFixture();
        await using var connection = fixture.ConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";

        var tables = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tables.Add(reader.GetString(0));

        tables.Should().Contain(["attachment", "client", "client_work_category",
            "invoice", "invoice_line", "setting", "work_category", "work_entry"]);
    }

    [Fact]
    public async Task InitializeAsync_RunTwice_IsIdempotent()
    {
        using var fixture = new SqliteTestFixture();
        var initializer = new SchemaInitializer(fixture.ConnectionFactory, NullLogger<SchemaInitializer>.Instance);

        var act = async () => await initializer.InitializeAsync();

        await act.Should().NotThrowAsync();
    }
}
