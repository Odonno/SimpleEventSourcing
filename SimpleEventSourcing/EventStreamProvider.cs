using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing
{
    public interface IEventStreamProviderStorageLayer<TEvent>
        where TEvent : StreamedEvent
    {
        Task<IEnumerable<EventStream<TEvent>>> GetAllStreamsAsync();
        Task<EventStream<TEvent>> GetStreamAsync(string streamId);
    }

    public interface IEventStreamProviderMessagingLayer<TEvent>
        where TEvent : StreamedEvent
    {
        IObservable<EventStream<TEvent>> ListenForNewStreams();
    }

    /// <summary>
    /// A provider of Event Stream, where all streams are stored.
    /// </summary>
    /// <typeparam name="TEvent">Type of events stored in the stream.</typeparam>
    public interface IEventStreamProvider<TEvent>
        where TEvent : StreamedEvent
    {
        IEventStreamProviderStorageLayer<TEvent> StorageProvider { get; }
        IEventStreamProviderMessagingLayer<TEvent> MessagingProvider { get; }

        Task<IEnumerable<EventStream<TEvent>>> GetAllStreamsAsync();
        Task<EventStream<TEvent>> GetStreamAsync(string streamId);
        IObservable<EventStream<TEvent>> ListenForNewStreams();
    }
}
