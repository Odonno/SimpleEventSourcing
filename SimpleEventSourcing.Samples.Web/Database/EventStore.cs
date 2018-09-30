using Dapper;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static SimpleEventSourcing.Samples.Web.DatabaseConfiguration;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public class AppEventStore : EventStore
    {
        private readonly Subject<EventInfo> _savedEventInfoSubject = new Subject<EventInfo>();

        public IObservable<EventInfo> ObserveEventInfoSaved()
        {
            return _savedEventInfoSubject.DistinctUntilChanged();
        }

        public override void Dispatch(object @event)
        {
            using (var connection = GetEventsDatabaseConnection())
            {
                var newEventInfo = connection.Query<EventInfo>(
                    @"
                    INSERT INTO [Event]
                    ([EventName], [Data], [CreatedDate])
                    VALUES (@EventName, @Data, @CreatedDate);

                    SELECT * FROM [Event] ORDER BY [Id] DESC LIMIT 1;
                    ",
                    new { @EventName = @event.GetType().Name, Data = JsonConvert.SerializeObject(@event), CreatedDate = DateTime.Now }
                )
                .Single();

                _savedEventInfoSubject.OnNext(newEventInfo);
            }

            base.Dispatch(@event);
        }
    }
}
