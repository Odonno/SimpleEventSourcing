using SimpleEventSourcing.InMemory;
using SimpleEventSourcing.UnitTests.Models;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleEventSourcing.UnitTests
{
    public class EventStoreTest
    {
        [Fact]
        public async Task CanApplyCommand()
        {
            // Arrange
            var streamProvider = new CustomEventStreamProvider<StreamedEvent>();

            var eventStore = EventStoreBuilder<StreamedEvent>
                .New()
                .WithStreamProvider(streamProvider)
                .WithApplyFunction(new AddItemInCartApplyFunction())
                .WithApplyFunction(new RemoveItemFromCartApplyFunction())
                .WithApplyFunction(new ResetCartApplyFunction())
                .Build();

            // Act
            await eventStore.ApplyAsync(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 45
            });

            // Assert
            var allStreams = await streamProvider.GetAllStreamsAsync();
            Assert.Single(allStreams);

            var cartStream = allStreams.Single();
            Assert.Equal("cart", cartStream.Id);

            var events = await cartStream.GetAllEventsAsync();
            Assert.Single(events);

            var firstEvent = events.Single();
            Assert.Equal(nameof(ItemAddedInCart), firstEvent.EventName);
        }

        [Fact]
        public async Task CanApplyMultipleCommands()
        {
            // Arrange
            var streamProvider = new CustomEventStreamProvider<StreamedEvent>();

            var eventStore = EventStoreBuilder<StreamedEvent>
                .New()
                .WithStreamProvider(streamProvider)
                .WithApplyFunction(new AddItemInCartApplyFunction())
                .WithApplyFunction(new RemoveItemFromCartApplyFunction())
                .WithApplyFunction(new ResetCartApplyFunction())
                .Build();

            // Act
            await eventStore.ApplyAsync(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 45
            });
            await eventStore.ApplyAsync(new ResetCartCommand());

            // Assert
            var allStreams = await streamProvider.GetAllStreamsAsync();
            Assert.Single(allStreams);

            var cartStream = allStreams.Single();
            Assert.Equal("cart", cartStream.Id);

            var events = await cartStream.GetAllEventsAsync();
            Assert.Equal(2, events.Count());

            var firstEvent = events.ElementAt(0);
            Assert.Equal(nameof(ItemAddedInCart), firstEvent.EventName);

            var secondEvent = events.ElementAt(1);
            Assert.Equal(nameof(CartReset), secondEvent.EventName);
        }
    }
}
