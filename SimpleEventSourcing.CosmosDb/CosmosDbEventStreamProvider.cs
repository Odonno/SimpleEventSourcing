using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing.CosmosDb
{
    public class CosmosDbEventStreamProvider<TEvent> : IRealtimeEventStreamProvider<TEvent>
        where TEvent : StreamedEvent
    {
        public IObservable<IEventStream<TEvent>> DetectNewStreams()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEventStream<TEvent>>> GetAllStreamsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEventStream<TEvent>> GetStreamAsync(string streamId)
        {
            throw new NotImplementedException();
        }
    }
}
