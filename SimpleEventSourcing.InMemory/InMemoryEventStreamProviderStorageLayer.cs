using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.InMemory
{
    public class InMemoryEventStreamProviderStorageLayer<TEvent> : IEventStreamProviderStorageLayer<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly ConcurrentDictionary<string, EventStream<TEvent>> _streams = new ConcurrentDictionary<string, EventStream<TEvent>>();
        private readonly Func<string, EventStream<TEvent>> _createNewStreamFunc;

        public InMemoryEventStreamProviderStorageLayer(
            Func<string, EventStream<TEvent>> createNewStreamFunc
        )
        {
            _createNewStreamFunc = createNewStreamFunc;
        }

        public Task<IEnumerable<EventStream<TEvent>>> GetAllStreamsAsync()
        {
            var allStreams = _streams
               .Select(x => x.Value)
               .ToList()
               .AsEnumerable();

            return Task.FromResult(allStreams);
        }

        public Task<EventStream<TEvent>> GetStreamAsync(string streamId)
        {
            var stream = _streams.GetOrAdd(
                streamId,
                _createNewStreamFunc
            );
            return Task.FromResult(stream);
        }
    }
}
