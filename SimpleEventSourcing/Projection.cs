﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace SimpleEventSourcing
{
    /// <summary>
    /// The base class for creating Projections (Read Model of an Event Sourcing architecture).
    /// This class does not contain a State and is mainly used to execute actions like updating a database after each event.
    /// </summary>
    public abstract class Projection<TEvent>
        where TEvent : StreamedEvent
    {
        protected readonly IEventStreamProvider<TEvent> _streamProvider;

        public Projection(IEventStreamProvider<TEvent> streamProvider)
        {
            _streamProvider = streamProvider;
        }

        protected abstract void Handle(TEvent @event, bool replayed = false);

        /// <summary>
        /// Replay a single event.
        /// </summary>
        /// <param name="event">The event to replay.</param>
        public virtual void Replay(TEvent @event)
        {
            Handle(@event, true);
        }
        /// <summary>
        /// Replay a set of events.
        /// </summary>
        /// <param name="events">The list of events to replay.</param>
        public virtual void Replay(IEnumerable<TEvent> events)
        {
            events.ToObservable().Subscribe(Replay);
        }
    }
}