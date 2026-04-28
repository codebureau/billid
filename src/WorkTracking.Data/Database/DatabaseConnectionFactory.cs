using Microsoft.Data.Sqlite;

namespace WorkTracking.Data.Database;

public class DatabaseConnectionFactory(string connectionString) : IDatabaseConnectionFactory
{
    public SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(connectionString);
        return connection;
    }

    public static string GetAppDataFolder()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Billable"
#if DEBUG
            , "dev"
#endif
        );
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string GetDefaultConnectionString()
    {
        var folder = GetAppDataFolder();
        return $"Data Source={Path.Combine(folder, "billable.db")}";
    }
}
