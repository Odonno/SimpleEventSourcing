﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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

    /// <summary>
    /// The base class for creating Event View (Read Model of an Event Sourcing architecture).
    /// The State is a real live-data updated whenever an observed event is pushed.
    /// </summary>
    public abstract class InMemoryEventView<TState> where TState : class, new()
    {
        private readonly Subject<TState> _stateSubject = new Subject<TState>();

        /// <summary>
        /// Gets the current state of the view.
        /// </summary>
        public TState State { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventView{TState}"/> class.
        /// </summary>
        /// <param name="events">The event stream to listen to in order to update the state.</param>
        /// <param name="initialState">The initial state to use in the view; if <c>null</c>, a default value is constructed using <c>new TState()</c>.</param>
        protected InMemoryEventView(IObservable<object> events, TState initialState = null)
        {
            State = initialState ?? new TState();

            events.Subscribe(@event =>
                {
                    State = Reduce(State, @event);
                    _stateSubject.OnNext(State);
                });
        }

        /// <summary>
        /// Observes the state of the store.
        /// </summary>
        /// <returns>An <see cref="IObservable{T}"/> that can be subscribed to in order to receive updates about state changes.</returns>
        public IObservable<TState> ObserveState()
        {
            return _stateSubject.DistinctUntilChanged();
        }
        /// <summary>
        /// Observes a value derived from the state of the view.
        /// </summary>
        /// <typeparam name="TPartial">The type of the partial state to be observed.</typeparam>
        /// <param name="selector">
        /// The mapping function that can be applied to get the desired partial state of type <typeparamref name="TPartial"/> from an instance of <typeparamref name="TState"/>.
        /// </param>
        /// <returns></returns>
        public IObservable<TPartial> ObserveState<TPartial>(Func<TState, TPartial> selector)
        {
            return _stateSubject.Select(selector).DistinctUntilChanged();
        }

        /// <summary>
        /// Reduces the specified state using the specified event and returns the new state. Does not mutate the current state of the view.
        /// Implementations should override this method to provide functionality specific to their use case.
        /// </summary>
        /// <param name="state">The state to reduce.</param>
        /// <param name="event">The event to use for reducing the specified state.</param>
        /// <returns>The state that results from applying <paramref name="action"/> on <paramref name="state"/>.</returns>
        protected abstract TState Reduce(TState state, object @event);
    }
}
