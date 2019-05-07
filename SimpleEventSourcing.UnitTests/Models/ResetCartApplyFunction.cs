using Converto;
using System.Threading.Tasks;
using static SimpleEventSourcing.Extensions;

namespace SimpleEventSourcing.UnitTests.Models
{
    public class ResetCartApplyFunction : IApplyFunction<ResetCartCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(ResetCartCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = command.ConvertTo<CartReset>();

            string streamId = "cart";
            var stream = await eventStreamProvider.GetStreamAsync(streamId);
            var currentPosition = await stream.GetCurrentPositionAsync();

            var events = List(@event);
            await stream.AppendEventsAsync(
                CreateStreamedEvents(streamId, currentPosition, events)
            );
        }
    }
}
