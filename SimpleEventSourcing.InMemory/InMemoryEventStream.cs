using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.InMemory
{
    public class InMemoryEventStream<TEvent> : IEventStream<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly ConcurrentDictionary<long, TEvent> _events = new ConcurrentDictionary<long, TEvent>();

        public string Id { get; }
        public EventStreamDetails Details { get; private set; }

        public InMemoryEventStream(string streamId)
        {
            Id = streamId;
        }

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
        }

        public Task<long?> GetCurrentPositionAsync()
        {
            return Task.FromResult(Details?.LastPosition);
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
    }
}
