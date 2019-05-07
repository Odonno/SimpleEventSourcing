using System.Collections.Generic;

namespace SimpleEventSourcing
{
    /// <summary>
    /// Build a new Event Store.
    /// </summary>
    /// <typeparam name="TEvent">Type of events stored in the stream.</typeparam>
    public class EventStoreBuilder<TEvent>
        where TEvent : StreamedEvent
    {
        private IEventStreamProvider<TEvent> _eventStreamProvider;
        private readonly List<object> _applyFunctions = new List<object>();
        private readonly List<IEvolveFunction<TEvent>> _evolveFunctions = new List<IEvolveFunction<TEvent>>();

        private EventStoreBuilder() { }

        public static EventStoreBuilder<TEvent> New()
        {
            return new EventStoreBuilder<TEvent>();
        }

        public EventStoreBuilder<TEvent> WithStreamProvider(IEventStreamProvider<TEvent> eventStreamProvider)
        {
            _eventStreamProvider = eventStreamProvider;
            return this;
        }

        public EventStoreBuilder<TEvent> WithApplyFunction(object applyFunction)
        {
            _applyFunctions.Add(applyFunction);
            return this;
        }

        public EventStoreBuilder<TEvent> WithEvolveFunction(IEvolveFunction<TEvent> evolveFunction)
        {
            _evolveFunctions.Add(evolveFunction);
            return this;
        }

        public EventStore<TEvent> Build()
        {
            return new EventStore<TEvent>(_applyFunctions, _evolveFunctions, _eventStreamProvider);
        }
    }
}
