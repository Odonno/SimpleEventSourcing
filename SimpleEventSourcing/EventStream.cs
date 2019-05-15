using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing
{
    public interface IEventStreamStorageLayer<TEvent>
        where TEvent : StreamedEvent
    {
        Task<long?> GetCurrentPositionAsync();
        Task<IEnumerable<TEvent>> GetAllEventsAsync();
        Task<TEvent> GetEventAsync(string eventId);
        Task<TEvent> GetEventAsync(int position);
        Task AppendEventAsync(TEvent @event);
        Task AppendEventsAsync(IEnumerable<TEvent> events);
    }

    public interface IEventStreamMessagingLayer<TEvent>
        where TEvent : StreamedEvent
    {
        IObservable<TEvent> ListenForNewEvents(bool isNewStream);
    }

    /// <summary>
    /// An Event Stream that contains a list of events based on a related id.
    /// </summary>
    /// <typeparam name="TEvent">Type of events stored in the stream.</typeparam>
    public sealed class EventStream<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly IEventStreamStorageLayer<TEvent> _storageLayer;
        private readonly IEventStreamMessagingLayer<TEvent> _messagingLayer;

        public string Id { get; }

        public EventStream(
            string id,
            IEventStreamStorageLayer<TEvent> storageLayer,
            IEventStreamMessagingLayer<TEvent> messagingLayer
        )
        {
            Id = id;
            _storageLayer = storageLayer;
            _messagingLayer = messagingLayer;
        }

        public Task<long?> GetCurrentPositionAsync()
            => _storageLayer.GetCurrentPositionAsync();
        public Task<IEnumerable<TEvent>> GetAllEventsAsync()
            => _storageLayer.GetAllEventsAsync();
        public Task<TEvent> GetEventAsync(string eventId)
            => _storageLayer.GetEventAsync(eventId);
        public Task<TEvent> GetEventAsync(int position)
            => _storageLayer.GetEventAsync(position);
        public Task AppendEventAsync(TEvent @event)
            => _storageLayer.AppendEventAsync(@event);
        public Task AppendEventsAsync(IEnumerable<TEvent> events)
            => _storageLayer.AppendEventsAsync(events);
        public IObservable<TEvent> ListenForNewEvents(bool isNewStream)
            => _messagingLayer.ListenForNewEvents(isNewStream);
    }
}
