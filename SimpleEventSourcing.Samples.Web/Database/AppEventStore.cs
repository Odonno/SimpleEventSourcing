using Dapper;
using System;
using System.Collections.Generic;
using static SimpleEventSourcing.Samples.Web.Database.Configuration;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public class AppEventStore : EventStore<SimpleEvent>
    {
        public AppEventStore(IObservable<IEnumerable<SimpleEvent>> eventAggregates) : base(eventAggregates)
        {
        }

        protected override void Persist(SimpleEvent @event)
        {
            using (var connection = GetEventsDatabaseConnection())
            {
                connection.Execute(
                    @"
                    INSERT INTO [Event]
                    ([EventName], [Data], [Metadata])
                    VALUES (@EventName, @Data, @Metadata)
                    ",
                    @event
                );
            }
        }
    }
}
