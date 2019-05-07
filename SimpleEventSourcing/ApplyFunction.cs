using System.Threading.Tasks;

namespace SimpleEventSourcing
{
    /// <summary>
    /// A function that when a command is triggered will push new events in event streams.
    /// </summary>
    /// <typeparam name="TCommand">The type of command triggered.</typeparam>
    /// <typeparam name="TEvent">The type of event stored.</typeparam>
    public interface IApplyFunction<TCommand, TEvent>
        where TCommand : class, new()
        where TEvent : StreamedEvent
    {
        Task ExecuteAsync(TCommand command, IEventStreamProvider<TEvent> eventStreamProvider);
    }
}
