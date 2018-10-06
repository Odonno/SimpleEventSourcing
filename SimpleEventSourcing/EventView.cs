using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace SimpleEventSourcing
{
    /// <summary>
    /// The base class for creating Event View (Read Model of an Event Sourcing architecture).
    /// This class does not contain a State and is mainly used to execute actions like updating a database after each event.
    /// </summary>
    public abstract class EventView
    {
        protected EventView(IObservable<object> events)
        {
            events.Subscribe(@event => Handle(@event));
        }

        /// <summary>
        /// Replay a single event.
        /// </summary>
        /// <param name="event">The event to replay.</param>
        public virtual void Replay(object @event)
        {
            Handle(@event, true);
        }
        /// <summary>
        /// Replay a set of events.
        /// </summary>
        /// <param name="events">The list of events to replay.</param>
        public virtual void Replay(IEnumerable<object> events)
        {
            events.ToObservable().Subscribe(Replay);
        }

        protected abstract void Handle(object @event, bool replayed = false);
    }
}
