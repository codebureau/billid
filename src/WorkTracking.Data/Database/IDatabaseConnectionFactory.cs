using Microsoft.Data.Sqlite;

namespace WorkTracking.Data.Database;

public interface IDatabaseConnectionFactory
{
    SqliteConnection CreateConnection();
}
