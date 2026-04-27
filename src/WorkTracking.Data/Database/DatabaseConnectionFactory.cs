using Microsoft.Data.Sqlite;

namespace WorkTracking.Data.Database;

public class DatabaseConnectionFactory(string connectionString) : IDatabaseConnectionFactory
{
    public SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(connectionString);
        return connection;
    }

    public static string GetDefaultConnectionString()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Billable");
        Directory.CreateDirectory(folder);
#if DEBUG
        const string dbName = "billable-dev.db";
#else
        const string dbName = "billable.db";
#endif
        return $"Data Source={Path.Combine(folder, dbName)}";
    }
}
