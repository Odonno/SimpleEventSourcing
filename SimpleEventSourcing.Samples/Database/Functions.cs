using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using static SimpleEventSourcing.Samples.DatabaseConfiguration;

namespace SimpleEventSourcing.Samples.Database
{
    public static class Functions
    {
        public static void CreateEventsDatabase()
        {
            if (!File.Exists(EventsDatabaseFilePath))
            {
                SQLiteConnection.CreateFile(EventsDatabaseFilePath);
            }

            using (var connection = GetEventsDatabaseConnection())
            {
                connection.Execute(
                    @"CREATE TABLE IF NOT EXISTS [Event] (
                        [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [EventName] VARCHAR(200) NOT NULL,
                        [Data] TEXT NOT NULL,
                        [CreatedDate] DATETIME NOT NULL
                    )"
                );
            }
        }

        public static void CreateViewsDatabase(int version)
        {
            if (!File.Exists(ViewsDatabaseFilePath))
            {
                SQLiteConnection.CreateFile(ViewsDatabaseFilePath);
            }

            using (var connection = GetViewsDatabaseConnection())
            {
                connection.Execute(
                   @"
                    CREATE TABLE IF NOT EXISTS [Version] (
                        [Value] INTEGER NOT NULL
                    )"
                );

                int? currentVersion = connection
                    .Query<int?>("SELECT [Value] FROM [Version]")
                    .FirstOrDefault();

                bool shouldRecreateDatabase = !currentVersion.HasValue || currentVersion.Value != version;
                if (shouldRecreateDatabase)
                {
                    connection.Execute(
                       @"
                        DROP TABLE IF EXISTS [Cart];
                        DROP TABLE IF EXISTS [Item];
                        "
                    );

                    connection.Execute(
                        @"
                        CREATE TABLE IF NOT EXISTS [Item] (
                            [Name] VARCHAR(200) NOT NULL PRIMARY KEY,
                            [UnitCost] DECIMAL(12, 2) NOT NULL
                        );

                        INSERT INTO [Item]
                        VALUES ('Book', 30);
                        INSERT INTO [Item]
                        VALUES ('Car', 14000);
                        INSERT INTO [Item]
                        VALUES ('Candy', 0.8);"
                    );

                    if (version == 1)
                    {
                        connection.Execute(
                            @"
                            CREATE TABLE IF NOT EXISTS [Cart] (
                                [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                [ItemName] VARCHAR(200) NOT NULL,
                                [NumberOfUnits] INTEGER NOT NULL
                            );"
                        );
                    }
                    if (version == 2)
                    {
                        connection.Execute(
                            @"
                            CREATE TABLE IF NOT EXISTS [Cart] (
                                [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                [ItemName] VARCHAR(200) NOT NULL,
                                [NumberOfUnits] INTEGER NOT NULL,
                                [CreatedDate] DATETIME NOT NULL,
                                [UpdatedDate] DATETIME NULL
                            );"
                        );
                    }

                    connection.Execute("DELETE FROM [Version]");
                    connection.Execute("INSERT INTO [Version] VALUES (@Version)", new { Version = version });
                }
            }
        }

        public static IEnumerable<object> GetEventsFromDatabase()
        {
            using (var connection = GetEventsDatabaseConnection())
            {
                return connection
                    .Query<EventInfo>("SELECT * FROM [Event] ORDER BY [Id] ASC")
                    .Select(eventInfo =>
                    {
                        var type = Type.GetType(eventInfo.EventName);
                        return JsonConvert.DeserializeObject(eventInfo.Data);
                    });
            }
        }

        public static void RemoveEventsDatabase()
        {
            if (File.Exists(EventsDatabaseFilePath))
            {
                File.Delete(EventsDatabaseFilePath);
            }
        }

        public static void RemoveViewsDatabase()
        {
            if (File.Exists(ViewsDatabaseFilePath))
            {
                File.Delete(ViewsDatabaseFilePath);
            }
        }

        public static decimal GetTotalCostInCart()
        {
            using (var connection = GetViewsDatabaseConnection())
            {
                return connection.Query<decimal>(
                    @"
                    SELECT SUM(item.[UnitCost] * cart.[NumberOfUnits]) 
                    FROM [Cart] cart
                    INNER JOIN [Item] item ON item.[Name] = cart.[ItemName]
                    ")
                    .Single();
            }
        }

        public static IEnumerable<Cart> GetCart()
        {
            using (var connection = GetViewsDatabaseConnection())
            {
                return connection.Query<Cart>("SELECT * FROM [Cart]");
            }
        }
    }
}
