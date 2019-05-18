using SimpleEventSourcing.InMemory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UnitTests.Models
{
    public class CustomEventStreamProvider<TEvent> : IEventStreamProvider<TEvent>
        where TEvent : StreamedEvent, new()
    {
        private readonly Subject<EventStream<TEvent>> _streamSubject = new Subject<EventStream<TEvent>>();

        public IEventStreamProviderStorageLayer<TEvent> StorageProvider { get; }
        public IEventStreamProviderMessagingLayer<TEvent> MessagingProvider { get; }

        public CustomEventStreamProvider()
        {
            EventStream<TEvent> createNewStreamFunc(string streamId)
            {
                var storageLayer = new CustomEventStreamStorageLayer<TEvent>();
                var messagingLayer = new CustomEventStreamMessagingLayer<TEvent>(storageLayer.ListenForNewEvents());

                var stream = new EventStream<TEvent>(
                    streamId,
                    storageLayer,
                    messagingLayer
                );
                _streamSubject.OnNext(stream);

                return stream;
            }

            StorageProvider = new InMemoryEventStreamProviderStorageLayer<TEvent>(createNewStreamFunc);
        }

        public Task<IEnumerable<EventStream<TEvent>>> GetAllStreamsAsync()
            => StorageProvider.GetAllStreamsAsync();
        public Task<EventStream<TEvent>> GetStreamAsync(string streamId)
            => StorageProvider.GetStreamAsync(streamId);
        public IObservable<EventStream<TEvent>> ListenForNewStreams()
            => _streamSubject;
    }

    public class CustomEventStreamStorageLayer<TEvent> : IEventStreamStorageLayer<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly Subject<TEvent> _eventSubject = new Subject<TEvent>();
        private readonly ConcurrentDictionary<long, TEvent> _events = new ConcurrentDictionary<long, TEvent>();

        public EventStreamDetails Details { get; private set; }

        private void TryAppendEvent(TEvent @event)
        {
            bool success = _events.TryAdd(@event.Position, @event);

            if (!success)
                throw new Exception($"An event already exist on the same position. Position: {@event.Position}");

            Details = new EventStreamDetails
            {
                LastPosition = @event.Position,
                UpdatedAt = DateTime.Now
            };
            _eventSubject.OnNext(@event);
        }

        public Task AppendEventAsync(TEvent @event)
        {
            TryAppendEvent(@event);
            return Task.CompletedTask;
        }

        public Task AppendEventsAsync(IEnumerable<TEvent> events)
        {
            foreach (var @event in events)
            {
                TryAppendEvent(@event);
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TEvent>> GetAllEventsAsync()
        {
            var allEvents = _events
                .Select(x => x.Value)
                .OrderBy(e => e.Position)
                .ToList()
                .AsEnumerable();

            return Task.FromResult(allEvents);
        }

        public Task<long?> GetCurrentPositionAsync()
        {
            return Task.FromResult(Details?.LastPosition);
        }

        public Task<TEvent> GetEventAsync(string eventId)
        {
            var @event = _events
                .Select(x => x.Value)
                .SingleOrDefault(e => e.Id == eventId);

            return Task.FromResult(@event);
        }

        public Task<TEvent> GetEventAsync(int position)
        {
            return Task.FromResult(_events[position]);
        }

        public IObservable<TEvent> ListenForNewEvents()
        {
            return _eventSubject;
        }
    }

    public class CustomEventStreamMessagingLayer<TEvent> : IEventStreamMessagingLayer<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly IObservable<TEvent> _eventObservable;

        public CustomEventStreamMessagingLayer(IObservable<TEvent> eventObservable)
        {
            _eventObservable = eventObservable;
        }

        public IObservable<TEvent> ListenForNewEvents(bool isNewStream)
        {
            return _eventObservable;
        }
    }
}
