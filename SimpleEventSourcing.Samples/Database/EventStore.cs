using Dapper;
using Newtonsoft.Json;
using System;
using static SimpleEventSourcing.Samples.DatabaseConfiguration;

namespace SimpleEventSourcing.Samples.Database
{
    public class CartEventStore : EventStore
    {
        public override void Dispatch(object @event)
        {
            using (var connection = GetEventsDatabaseConnection())
            {
                connection.Execute(
                    @"
                    INSERT INTO [Event]
                    ([EventName], [Data], [CreatedDate])
                    VALUES (@EventName, @Data, @CreatedDate)
                    ",
                    new { @EventName = @event.GetType().Name, Data = JsonConvert.SerializeObject(@event), CreatedDate = DateTime.Now }
                );
            }

            base.Dispatch(@event);
        }
    }
}
