using SimpleEventSourcing.InMemory;
using SimpleEventSourcing.UnitTests.Models;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleEventSourcing.UnitTests
{
    public class EventViewTest
    {
        [Fact]
        public void CanReadInitialState()
        {
            // Arrange
            var streamProvider = new CustomEventStreamProvider<StreamedEvent>();

            var eventView = new TotalCostCartEventView(streamProvider);

            // Act

            // Assert
            Assert.Equal(0, eventView.State.TotalCost);
        }

        [Fact]
        public async Task CanObserveStateWithEventsOfTheSameType()
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

            var eventView = new TotalCostCartEventView(streamProvider);

            // Act
            int eventListenedCount = 0;
            TotalCostCartState lastState = null;

            eventView.ObserveState()
                .Subscribe(state =>
                {
                    eventListenedCount++;
                    lastState = state;
                });

            await eventStore.ApplyAsync(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 45
            });
            await eventStore.ApplyAsync(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 20
            });

            // Assert
            Assert.Equal(2, eventListenedCount);
            Assert.NotNull(lastState);
            Assert.Equal(65, lastState.TotalCost);
        }

        [Fact]
        public async Task CanObserveStateWithEventsOfDifferentTypes()
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

            var eventView = new TotalCostCartEventView(streamProvider);

            // Act
            int eventListenedCount = 0;
            TotalCostCartState lastState = null;

            eventView.ObserveState()
                .Subscribe(state =>
                {
                    eventListenedCount++;
                    lastState = state;
                });

            await eventStore.ApplyAsync(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 45
            });
            await eventStore.ApplyAsync(new ResetCartCommand());

            // Assert
            Assert.Equal(2, eventListenedCount);
            Assert.NotNull(lastState);
            Assert.Equal(0, lastState.TotalCost);
        }

        [Fact]
        public async Task CanObserveStateOfMultipleEventViews()
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

            var totalCostCartEventView = new TotalCostCartEventView(streamProvider);
            var ordersCartEventView = new OrdersCartEventView(streamProvider);

            // Act
            TotalCostCartState lastTotalCostCartState = null;
            OrdersCartState lastOrdersCartState = null;

            totalCostCartEventView.ObserveState()
                .Subscribe(state =>
                {
                    lastTotalCostCartState = state;
                });
            ordersCartEventView.ObserveState()
                .Subscribe(state =>
                {
                    lastOrdersCartState = state;
                });

            await eventStore.ApplyAsync(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 45
            });
            await eventStore.ApplyAsync(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 20
            });

            // Assert
            Assert.NotNull(lastTotalCostCartState);
            Assert.Equal(65, lastTotalCostCartState.TotalCost);

            Assert.NotNull(lastOrdersCartState);
            Assert.Equal(2, lastOrdersCartState.NumberOfItems);
            Assert.Single(lastOrdersCartState.Items);
            Assert.True(lastOrdersCartState.Items.ContainsKey("Book"));
            Assert.Equal(2, lastOrdersCartState.Items["Book"]);
        }

        [Fact]
        public async Task CanObserveStatePartially()
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

            var ordersCartEventView = new OrdersCartEventView(streamProvider);

            // Act
            int lastNumberOfItems = 0;

            ordersCartEventView.ObserveState(state => state.NumberOfItems)
                .Subscribe(numberOfItems =>
                {
                    lastNumberOfItems = numberOfItems;
                });

            await eventStore.ApplyAsync(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 45
            });
            await eventStore.ApplyAsync(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 20,
                NumberOfUnits = 2
            });

            // Assert
            Assert.Equal(3, lastNumberOfItems);
        }
    }
}
