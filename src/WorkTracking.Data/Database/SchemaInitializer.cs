using System.Reflection;
using Microsoft.Extensions.Logging;

namespace WorkTracking.Data.Database;

public class SchemaInitializer(IDatabaseConnectionFactory connectionFactory, ILogger<SchemaInitializer> logger)
{
    private const string SchemaVersionKey = "schema_version";

    public async Task InitializeAsync()
    {
        await ApplyBaselineSchemaAsync();
        await ApplyMigrationsAsync();
        logger.LogInformation("Database schema initialized.");
    }

    private async Task ApplyBaselineSchemaAsync()
    {
        var sql = ReadEmbeddedScript("schema.sql");

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        foreach (var statement in SplitStatements(sql))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = statement;
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task ApplyMigrationsAsync()
    {
        var currentVersion = await GetSchemaVersionAsync();
        var migrations = DiscoverMigrations(currentVersion);

        foreach (var (version, resourceName) in migrations)
        {
            logger.LogInformation("Applying migration {Version}: {Resource}", version, resourceName);
            var sql = ReadEmbeddedScript(resourceName);

            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                foreach (var statement in SplitStatements(sql))
                {
                    await using var command = connection.CreateCommand();
                    command.Transaction = (Microsoft.Data.Sqlite.SqliteTransaction)transaction;
                    command.CommandText = statement;
                    await command.ExecuteNonQueryAsync();
                }

                await using var versionCmd = connection.CreateCommand();
                versionCmd.Transaction = (Microsoft.Data.Sqlite.SqliteTransaction)transaction;
                versionCmd.CommandText = "INSERT OR REPLACE INTO setting (key, value) VALUES ($key, $value)";
                versionCmd.Parameters.AddWithValue("$key", SchemaVersionKey);
                versionCmd.Parameters.AddWithValue("$value", version.ToString());
                await versionCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                logger.LogInformation("Migration {Version} applied.", version);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    private async Task<int> GetSchemaVersionAsync()
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        // setting table may not exist yet on a brand-new DB before baseline runs
        command.CommandText = """
            SELECT value FROM setting WHERE key = $key
            """;
        command.Parameters.AddWithValue("$key", SchemaVersionKey);

        try
        {
            var result = await command.ExecuteScalarAsync();
            return result is null or DBNull ? 1 : int.Parse((string)result);
        }
        catch
        {
            return 1;
        }
    }

    private static IReadOnlyList<(int Version, string ResourceName)> DiscoverMigrations(int currentVersion)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var migrations = new List<(int, string)>();

        foreach (var name in assembly.GetManifestResourceNames())
        {
            var fileName = name.Split('.').TakeLast(2).First(); // e.g. migration_0002_add_client_notes
            if (!fileName.StartsWith("migration_", StringComparison.OrdinalIgnoreCase)) continue;

            var parts = fileName.Split('_');
            if (parts.Length < 2 || !int.TryParse(parts[1], out var version)) continue;
            if (version <= currentVersion) continue;

            migrations.Add((version, name));
        }

        return [.. migrations.OrderBy(m => m.Item1)];
    }

    private static string ReadEmbeddedScript(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .Single(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static IEnumerable<string> SplitStatements(string sql)
        => sql.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
              .Where(s => !string.IsNullOrWhiteSpace(s));
}
