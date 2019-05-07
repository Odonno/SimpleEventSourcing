using System.Threading.Tasks;

namespace SimpleEventSourcing
{
    /// <summary>
    /// A function that is triggered when an event is stored inside a stream and will eventually push new events.
    /// </summary>
    /// <typeparam name="TEvent">The type of event stored.</typeparam>
    public interface IEvolveFunction<TEvent>
        where TEvent : StreamedEvent
    {
        bool OfEvent(TEvent @event);
        Task<bool> ShouldListenStreamsAsync(string streamId);
        Task ExecuteAsync(TEvent @event, IEventStreamProvider<TEvent> eventStreamProvider);
    }
}
