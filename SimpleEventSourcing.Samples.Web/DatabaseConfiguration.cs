using Microsoft.Data.Sqlite;
using System.Data;

namespace SimpleEventSourcing.Samples.Web
{
    public static class DatabaseConfiguration
    {
        public const string EventsDatabaseFilePath = "EventsDatabase.db";
        public const string ViewsDatabaseFilePath = "ViewsDatabase.db";

        public static SqliteConnection GetEventsDatabaseConnection()
        {
            var connection = new SqliteConnection($"Data Source={EventsDatabaseFilePath}");
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        public static SqliteConnection GetViewsDatabaseConnection()
        {
            var connection = new SqliteConnection($"Data Source={ViewsDatabaseFilePath}");
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }
    }
}
