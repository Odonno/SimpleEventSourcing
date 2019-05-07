using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing.CosmosDb
{
    public class CosmosDbEventStream<TEvent> : IRealtimeEventStream<TEvent>
        where TEvent : StreamedEvent
    {
        public string Id { get; }

        public CosmosDbEventStream(string streamId)
        {
            Id = streamId;
        }

        public Task<long?> GetCurrentPositionAsync()
        {
            throw new NotImplementedException();
        }

        public Task AppendEventAsync(TEvent @event)
        {
            throw new NotImplementedException();
        }

        public Task AppendEventsAsync(IEnumerable<TEvent> events)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEvent>> GetAllEventsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TEvent> GetEventAsync(string eventId)
        {
            throw new NotImplementedException();
        }

        public Task<TEvent> GetEventAsync(int position)
        {
            throw new NotImplementedException();
        }

        public IObservable<TEvent> ListenForNewEvents(bool isNewStream)
        {
            throw new NotImplementedException();
        }
    }
}
