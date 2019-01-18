using Dapper;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SimpleEventSourcing.Samples.EventStore
{
    public class AppEventStore : EventStore<AppEvent>
    {
        public AppEventStore(IObservable<IEnumerable<AppEvent>> eventAggregates) : base(eventAggregates)
        {
            HandleDatabaseCreation();
        }

        private static SqliteConnection GetDatabaseConnection()
        {
            var connection = new SqliteConnection("Data Source=../EventsDatabase.db");
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        private static void HandleDatabaseCreation()
        {
            using (var connection = GetDatabaseConnection())
            {
                connection.Execute(
                    @"
                    CREATE TABLE IF NOT EXISTS [Event] (
                        [Id] VARCHAR(36) NOT NULL PRIMARY KEY,
                        [EventName] DATETIME NOT NULL,
                        [Data] INTEGER NOT NULL,
                        [Metadata] INTEGER NOT NULL
                    );
                    "
                );
            }
        }

        protected override IEnumerable<AppEvent> Persist(IEnumerable<AppEvent> events)
        {
            var persistedEvents = new List<AppEvent>();

            using (var connection = GetDatabaseConnection())
            using (var transaction = connection.BeginTransaction())
            {
                int maxEventNumber = connection.Query<int>(
                    @"
                    SELECT IFNULL(MAX([Number]), 0)
                    FROM [Event]
                    "
                ).Single();

                foreach (var @event in events)
                {
                    string newEventId = Guid.NewGuid().ToString();

                    connection.Execute(
                        @"
                        INSERT INTO [Event]
                        ([Id], [EventName], [Data], [Metadata])
                        VALUES (@Id, @EventName, @Data, @Metadata);
                        ",
                        new
                        {
                            Id = newEventId,
                            EventName = @event.EventName,
                            Data = JsonConvert.SerializeObject(@event.Data),
                            Metadata = JsonConvert.SerializeObject(@event.Metadata)
                        },
                        transaction
                    );

                    persistedEvents.Add(new AppEvent
                    {
                        Id = newEventId,
                        EventName = @event.EventName,
                        Data = @event.Data,
                        Metadata = @event.Metadata
                    });
                }

                transaction.Commit();
            }

            return persistedEvents;
        }
    }
}
