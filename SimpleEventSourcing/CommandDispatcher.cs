using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace SimpleEventSourcing
{
    /// <summary>
    /// The base class for dispatching commands from the system, which leads to creation of events.
    /// </summary>
    public abstract class CommandDispatcher<TCommand, TEvent> 
        where TCommand : class, new() 
        where TEvent : class, new()
    {
        private readonly Subject<IEnumerable<TEvent>> _eventAggregateSubject = new Subject<IEnumerable<TEvent>>();

        /// <summary>
        /// Dispatches a command that leads to the creation of events.
        /// </summary>
        /// <param name="command">The command sent from the application.</param>
        public void Dispatch(TCommand command)
        {
            _eventAggregateSubject.OnNext(Convert(command));
        }

        /// <summary>
        /// Observes aggregates of events created from commands.
        /// </summary>
        /// <returns>An <see cref="IObservable{IEnumerable{TEvent}}"/> that can be subscribed to in order to receive updates about aggregates of events dispatched.</returns>
        public IObservable<IEnumerable<TEvent>> ObserveEventAggregate()
        {
            return _eventAggregateSubject;
        }

        /// <summary>
        /// Create a list of events from a command.
        /// Implementations should override this method to provide functionality specific to their use case.
        /// </summary>
        /// <param name="command">The command sent from the application.</param>
        /// <returns>Returns the list of events.</returns>
        protected abstract IEnumerable<TEvent> Convert(TCommand command);
    }
}
