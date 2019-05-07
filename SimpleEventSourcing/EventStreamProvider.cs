using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing
{
    /// <summary>
    /// A provider of Event Stream, where all streams are stored.
    /// </summary>
    /// <typeparam name="TEvent">Type of events stored in the stream.</typeparam>
    public interface IEventStreamProvider<TEvent>
        where TEvent : StreamedEvent
    {
        Task<IEventStream<TEvent>> GetStreamAsync(string streamId);
        Task<IEnumerable<IEventStream<TEvent>>> GetAllStreamsAsync();
    }

    /// <summary>
    /// A provider of Event Stream, where all streams are stored.
    /// With ability to detect when new streams are created in realtime.
    /// </summary>
    /// <typeparam name="TEvent">Type of events stored in the stream.</typeparam>
    public interface IRealtimeEventStreamProvider<TEvent> : IEventStreamProvider<TEvent>
        where TEvent : StreamedEvent
    {
        IObservable<IEventStream<TEvent>> DetectNewStreams();
    }
}
