﻿using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SimpleEventSourcing
{
    /// <summary>
    /// The base class for creating Projection (Read Model of an Event Sourcing architecture).
    /// The State is a real live-data updated whenever an observed event is pushed.
    /// </summary>
    public abstract class EntityProjection<TEvent, TState>
        where TEvent : StreamedEvent
        where TState : class, new()
    {
        private readonly Subject<TState> _stateSubject = new Subject<TState>();
        protected readonly IEventStreamProvider<TEvent> _streamProvider;

        /// <summary>
        /// Gets the current state of the view.
        /// </summary>
        public TState State { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventView{TState}"/> class.
        /// </summary>
        /// <param name="streamProvider">The provider of event streams.</param>
        /// <param name="initialState">The initial state to use in the view; if <c>null</c>, a default value is constructed using <c>new TState()</c>.</param>
        protected EntityProjection(IEventStreamProvider<TEvent> streamProvider, TState initialState = null)
        {
            _streamProvider = streamProvider;
            State = initialState ?? new TState();
        }

        protected void Handle(TEvent @event)
        {
            var state = Reduce(State, @event);
            State = state;
            _stateSubject.OnNext(state);
        }

        /// <summary>
        /// Observes the state of the store.
        /// </summary>
        /// <returns>An <see cref="IObservable{TState}"/> that can be subscribed to in order to receive updates about state changes.</returns>
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
        protected abstract TState Reduce(TState state, TEvent @event);
    }
}