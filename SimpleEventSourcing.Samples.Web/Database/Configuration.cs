using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public static class Configuration
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

        public static void CreateEventsDatabase()
        {
            using (var connection = GetEventsDatabaseConnection())
            {
                connection.Execute(
                    @"CREATE TABLE IF NOT EXISTS [Event] (
                        [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [EventName] VARCHAR(200) NOT NULL,
                        [Data] TEXT NOT NULL,
                        [Metadata] TEXT NULL
                    )"
                );
            }
        }

        public static void CreateViewsDatabase()
        {
            using (var connection = GetViewsDatabaseConnection())
            {
                connection.Execute(
                    @"
                    CREATE TABLE IF NOT EXISTS [Cart] (
                        [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [ItemId] INTEGER NOT NULL,
                        [Quantity] INTEGER NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS [Item] (
                        [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [Name] VARCHAR(200) NOT NULL,
                        [Price] DECIMAL(12, 2) NOT NULL,
                        [RemainingQuantity] INTEGER NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS [Order] (
                        [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [Number] INTEGER NOT NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [IsConfirmed] INTEGER NOT NULL,
                        [IsCanceled] INTEGER NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS [ItemOrdered] (
                        [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [OrderId] INTEGER NOT NULL,
                        [ItemId] INTEGER NOT NULL,
                        [Quantity] INTEGER NOT NULL,
                        [Price] DECIMAL(12, 2) NOT NULL
                    );
                    "
                );
            }
        }
    }
}
