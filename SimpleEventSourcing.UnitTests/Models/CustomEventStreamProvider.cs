using SimpleEventSourcing.InMemory;
using System;
using System.Collections.Generic;
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
                var stream = new EventStream<TEvent>(
                    streamId,
                    new InMemoryEventStreamStorageLayer<TEvent>(),
                    null
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
}
