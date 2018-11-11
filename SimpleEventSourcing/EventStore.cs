using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SimpleEventSourcing
{
    /// <summary>
    /// The base class for creating Event Store (Write Model of an Event Sourcing architecture).
    /// </summary>
    public abstract class EventStore<TEvent> 
        where TEvent : class, new()
    {
        private readonly Subject<TEvent> _eventSubject = new Subject<TEvent>();

        protected EventStore(IObservable<IEnumerable<TEvent>> eventAggregates)
        {
            eventAggregates.Subscribe(events => Push(events));
        }

        /// <summary>
        /// Push the specified events to a persistent layer like a database.
        /// </summary>
        /// <param name="events">The list of events to store.</param>
        public void Push(IEnumerable<TEvent> events)
        {
            foreach (var @event in events)
            {
                Persist(@event);
            }
            foreach (var @event in events)
            {
                _eventSubject.OnNext(@event);
            }
        }

        /// <summary>
        /// Observes events being pushed in the store.
        /// </summary>
        /// <returns>An <see cref="IObservable{TEvent}"/> that can be subscribed to in order to receive updates about events pushed in the store.</returns>
        public IObservable<TEvent> ObserveEvent()
        {
            return _eventSubject;
        }
        /// <summary>
        /// Observes events of a specific type being pushed in the store.
        /// </summary>
        /// <typeparam name="T">The type of events that the subscriber is interested in.</typeparam>
        /// <returns>
        /// An <see cref="IObservable{T}"/> that can be subscribed to in order to receive updates whenever an event of <typeparamref name="T"/> is pushed in the store.
        /// </returns>
        public IObservable<T> ObserveEvent<T>() where T : class
        {
            return _eventSubject.OfType<T>();
        }

        /// <summary>
        /// Save a new event in a persistent layer.
        /// Implementations should override this method to provide functionality specific to their use case.
        /// </summary>
        /// <param name="@event">The event to persist.</param>
        protected virtual void Persist(TEvent @event)
        {
            // No persistent layer by default
        }
    }
}
