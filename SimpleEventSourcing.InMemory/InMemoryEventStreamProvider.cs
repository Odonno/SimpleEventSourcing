using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.InMemory
{
    public class InMemoryEventStreamProvider<TEvent> : IEventStreamProvider<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly ConcurrentDictionary<string, IEventStream<TEvent>> _streams = new ConcurrentDictionary<string, IEventStream<TEvent>>();

        public Task<IEnumerable<IEventStream<TEvent>>> GetAllStreamsAsync()
        {
            var allStreams = _streams
                .Select(x => x.Value)
                .ToList()
                .AsEnumerable();

            return Task.FromResult(allStreams);
        }

        public Task<IEventStream<TEvent>> GetStreamAsync(string streamId)
        {
            var stream = _streams.GetOrAdd(
                streamId,
                CreateNewStream
            );

            return Task.FromResult(stream);
        }

        private Func<string, IEventStream<TEvent>> CreateNewStream =>
            (string streamId) => new InMemoryEventStream<TEvent>(streamId);
    }
}
