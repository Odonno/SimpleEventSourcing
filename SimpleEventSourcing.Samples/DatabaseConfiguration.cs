using System.Data;
using System.Data.SQLite;

namespace SimpleEventSourcing.Samples
{
    public static class DatabaseConfiguration
    {
        public const string EventsDatabaseFilePath = "./EventsDatabase.sqlite";
        public const string ViewsDatabaseFilePath = "./ViewsDatabase.sqlite";

        public static SQLiteConnection GetEventsDatabaseConnection()
        {
            var connection = new SQLiteConnection($"Data Source={EventsDatabaseFilePath};Version=3;");
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        public static SQLiteConnection GetViewsDatabaseConnection()
        {
            var connection = new SQLiteConnection($"Data Source={ViewsDatabaseFilePath};Version=3;");
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }
    }
}
