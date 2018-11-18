using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static SimpleEventSourcing.Samples.Web.Database.Configuration;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public class AppEventStore : EventStore<AppEvent>
    {
        public AppEventStore(IObservable<IEnumerable<AppEvent>> eventAggregates) : base(eventAggregates)
        {
        }

        protected override IEnumerable<AppEvent> Persist(IEnumerable<AppEvent> events)
        {
            var persistedEvents = new List<AppEvent>();

            using (var connection = GetEventsDatabaseConnection())
            using (var transaction = connection.BeginTransaction())
            {
                foreach (var @event in events)
                {
                    var newEventId = connection.Query<int>(
                        @"
                        INSERT INTO [Event]
                        ([EventName], [Data], [Metadata])
                        VALUES (@EventName, @Data, @Metadata);

                        SELECT last_insert_rowid();
                        ",
                        new
                        {
                            EventName = @event.EventName,
                            Data = JsonConvert.SerializeObject(@event.Data),
                            Metadata = JsonConvert.SerializeObject(@event.Metadata)
                        },
                        transaction
                    ).Single();

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
