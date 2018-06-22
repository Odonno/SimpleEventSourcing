using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SimpleEventSourcing
{
    /// <summary>
    /// The base class for creating Event Store (Write Model of an Event Sourcing architecture).
    /// </summary>
    public abstract class EventStore
    {
        private readonly Subject<object> _eventSubject = new Subject<object>();

        /// <summary>
        /// Dispatches the specified event to the store, which can lead to a storage action (for example in a database).
        /// Implementations should override this method to provide functionality specific to their use case.
        /// </summary>
        /// <param name="event">The event to be pushed in the store.</param>
        public virtual void Dispatch(object @event)
        {
            _eventSubject.OnNext(@event);
        }

        /// <summary>
        /// Observes events being pushed in the store.
        /// </summary>
        /// <returns>An <see cref="IObservable{T}"/> that can be subscribed to in order to receive updates about events pushed in the store.</returns>
        public IObservable<object> ObserveEvent()
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
    }
}
