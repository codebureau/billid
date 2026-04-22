using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using WorkTracking.Data.Database;

namespace WorkTracking.Tests.Data;

public class SqliteTestFixture : IDisposable
{
    private readonly string _dbPath;

    public string ConnectionString { get; }
    public IDatabaseConnectionFactory ConnectionFactory { get; }

    public SqliteTestFixture()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"worktracking_test_{Guid.NewGuid():N}.db");
        ConnectionString = $"Data Source={_dbPath}";
        ConnectionFactory = new DatabaseConnectionFactory(ConnectionString);

        var initializer = new SchemaInitializer(ConnectionFactory, NullLogger<SchemaInitializer>.Instance);
        initializer.InitializeAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
