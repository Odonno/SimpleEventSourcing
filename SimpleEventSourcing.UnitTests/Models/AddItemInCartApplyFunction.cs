using Converto;
using System.Threading.Tasks;
using static SimpleEventSourcing.Extensions;

namespace SimpleEventSourcing.UnitTests.Models
{
    public class AddItemInCartApplyFunction : IApplyFunction<AddItemInCartCommand, StreamedEvent>
    {
        public async Task ExecuteAsync(AddItemInCartCommand command, IEventStreamProvider<StreamedEvent> eventStreamProvider)
        {
            var @event = command.ConvertTo<ItemAddedInCart>();

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
