using Microsoft.Data.Sqlite;
using System.Data;

namespace SimpleEventSourcing.Samples.Orders
{
    public static class Configuration
    {
        public static SqliteConnection GetDatabaseConnection()
        {
            var connection = new SqliteConnection("Data Source=OrdersDatabase.db");
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }
    }
}
