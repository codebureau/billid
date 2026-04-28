using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WorkTracking.Data.Database;

namespace WorkTracking.Tests.Data.Migrations;

public class Migration0002Tests
{
    [Fact]
    public async Task Migration0002_AppliesCleanly_AndSeedsExportDefinition()
    {
        using var fixture = new SqliteTestFixture();

        await using var connection = fixture.ConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM setting WHERE key = 'export_definition'";

        var result = await command.ExecuteScalarAsync();

        result.Should().NotBeNull();
        var json = result as string;
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("IncludeDate");
        json.Should().Contain("IncludeClientName");
    }

    [Fact]
    public async Task Migration0002_RunTwice_IsIdempotent()
    {
        using var fixture = new SqliteTestFixture();
        var initializer = new SchemaInitializer(fixture.ConnectionFactory, NullLogger<SchemaInitializer>.Instance);

        var act = async () => await initializer.InitializeAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Migration0002_DoesNotOverwrite_ExistingExportDefinition()
    {
        using var fixture = new SqliteTestFixture();

        // Simulate a user-customised export definition already in the DB
        await using var connection = fixture.ConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var setCmd = connection.CreateCommand();
        setCmd.CommandText = "INSERT OR REPLACE INTO setting (key, value) VALUES ('export_definition', 'custom_value')";
        await setCmd.ExecuteNonQueryAsync();

        // Re-running the initializer should not overwrite it (INSERT OR IGNORE)
        var initializer = new SchemaInitializer(fixture.ConnectionFactory, NullLogger<SchemaInitializer>.Instance);
        await initializer.InitializeAsync();

        await using var getCmd = connection.CreateCommand();
        getCmd.CommandText = "SELECT value FROM setting WHERE key = 'export_definition'";
        var result = await getCmd.ExecuteScalarAsync();

        (result as string).Should().Be("custom_value");
    }
}
