using Converto;
using System.Threading.Tasks;
using static SimpleEventSourcing.Extensions;

namespace SimpleEventSourcing.UnitTests.Models
{
    public class RemoveItemFromCartApplyFunction : IApplyFunction<RemoveItemFromCartCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(RemoveItemFromCartCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = command.ConvertTo<ItemRemovedFromCart>();

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
