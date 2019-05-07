using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing
{
    /// <summary>
    /// An Event Stream that contains a list of events based on a related id.
    /// </summary>
    /// <typeparam name="TEvent">Type of events stored in the stream.</typeparam>
    public interface IEventStream<TEvent>
        where TEvent : StreamedEvent
    {
        string Id { get; }

        Task<long?> GetCurrentPositionAsync();
        Task<IEnumerable<TEvent>> GetAllEventsAsync();
        Task<TEvent> GetEventAsync(string eventId);
        Task<TEvent> GetEventAsync(int position);
        Task AppendEventAsync(TEvent @event);
        Task AppendEventsAsync(IEnumerable<TEvent> events);
    }

    /// <summary>
    /// An Event Stream that provide a way to listen to new events created in realtime.
    /// </summary>
    /// <typeparam name="TEvent">Type of events stored in the stream.</typeparam>
    public interface IRealtimeEventStream<TEvent> : IEventStream<TEvent>
        where TEvent : StreamedEvent
    {
        IObservable<TEvent> ListenForNewEvents(bool isNewStream);
    }
}
