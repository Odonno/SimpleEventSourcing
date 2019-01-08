using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Samples.Events
{
    public class AppEventStore : EventStore<AppEvent>
    {
        public AppEventStore(IObservable<IEnumerable<AppEvent>> eventAggregates) : base(eventAggregates)
        {
        }

        protected override IEnumerable<AppEvent> Persist(IEnumerable<AppEvent> events)
        {
            // TODO : Persist events in Event Store

            return base.Persist(events);
        }
    }
}
