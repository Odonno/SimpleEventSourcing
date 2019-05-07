using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing
{
    /// <summary>
    /// The "command center" to create an Event Store (Write Model of an Event Sourcing architecture).
    /// </summary>
    public sealed class EventStore<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly List<object> _applyFunctions;
        private readonly List<IEvolveFunction<TEvent>> _evolveFunctions;
        private readonly IEventStreamProvider<TEvent> _eventStreamProvider;

        private EventStore() { }
        internal EventStore(
            List<object> applyFunctions,
            List<IEvolveFunction<TEvent>> evolveFunctions,
            IEventStreamProvider<TEvent> eventStreamProvider
        )
        {
            _applyFunctions = applyFunctions;
            _evolveFunctions = evolveFunctions;
            _eventStreamProvider = eventStreamProvider;

            ListenStreamsToEvolve();
        }

        /// <summary>
        /// Evolve the Event Store by listening to the upcoming events from external streams of events.
        /// </summary>
        private void ListenStreamsToEvolve()
        {
            if (!_evolveFunctions.Any())
                return;

            if (_eventStreamProvider is IRealtimeEventStreamProvider<TEvent> realtimeEventStreamProvider)
            {
                realtimeEventStreamProvider
                    .DetectNewStreams()
                    .OfType<IRealtimeEventStream<TEvent>>()
                    .Subscribe(async stream =>
                    {
                        var shouldListenTasks = await Task.WhenAll(_evolveFunctions.Select(func => func.ShouldListenStreamsAsync(stream.Id)));
                        bool shouldListen = shouldListenTasks.Any();

                        if (!shouldListen)
                            return;

                        stream.ListenForNewEvents(true).Subscribe(async @event =>
                        {
                            var evolveFunctions = _evolveFunctions
                                .Where(func => func.OfEvent(@event))
                                .ToList();

                            foreach (var evolveFunction in evolveFunctions)
                            {
                                await evolveFunction.ExecuteAsync(@event, _eventStreamProvider);
                            }
                        });
                    });
            }
        }

        /// <summary>
        /// Apply a command that will be transformed into 1 or more events that will then be saved in the Event Store.
        /// </summary>
        /// <param name="command">The command to apply.</param>
        public async Task ApplyAsync<TCommand>(TCommand command)
            where TCommand : class, new()
        {
            var applyFunctions = _applyFunctions
                .OfType<IApplyFunction<TCommand, TEvent>>()
                .ToList();

            foreach (var applyFunction in applyFunctions)
            {
                await applyFunction.ExecuteAsync(command, _eventStreamProvider);
            }
        }
    }
}
