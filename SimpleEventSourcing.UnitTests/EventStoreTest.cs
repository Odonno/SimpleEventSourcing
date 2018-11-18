using System;
using System.Reactive.Linq;
using Xunit;

namespace SimpleEventSourcing.UnitTests
{
    public class EventStoreTest
    {
        [Fact]
        public void CanDispatchEvent()
        {
            // Arrange
            var commandDispatcher = new CartCommandDispatcher();
            var eventStore = new CartEventStore(commandDispatcher.ObserveEventAggregate());

            // Act
            commandDispatcher.Dispatch(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 45
            });

            // Assert
        }

        [Fact]
        public void CanObserveMultipleEventsOfTheSameType()
        {
            // Arrange
            var commandDispatcher = new CartCommandDispatcher();
            var eventStore = new CartEventStore(commandDispatcher.ObserveEventAggregate());

            // Act
            int eventListenedCount = 0;

            eventStore.ObserveEvent()
                .Subscribe(_ =>
                {
                    eventListenedCount++;
                });

            commandDispatcher.Dispatch(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 45
            });
            commandDispatcher.Dispatch(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 20
            });

            // Assert
            Assert.Equal(2, eventListenedCount);
        }

        [Fact]
        public void CanObserveMultipleEventsOfDifferentTypes()
        {
            // Arrange
            var commandDispatcher = new CartCommandDispatcher();
            var eventStore = new CartEventStore(commandDispatcher.ObserveEventAggregate());

            // Act
            int eventListenedCount = 0;
            SimpleEvent lastEvent = null;

            eventStore.ObserveEvent()
                .Subscribe(@event =>
                {
                    eventListenedCount++;
                    lastEvent = @event;
                });

            commandDispatcher.Dispatch(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 45
            });
            commandDispatcher.Dispatch(new ResetCartCommand());

            // Assert
            Assert.Equal(2, eventListenedCount);
            Assert.IsType<ResetCartCommand>(lastEvent.Data);
        }

        [Fact]
        public void CanObserveSingleEventType()
        {
            // Arrange
            var commandDispatcher = new CartCommandDispatcher();
            var eventStore = new CartEventStore(commandDispatcher.ObserveEventAggregate());

            // Act
            int eventListenedCount = 0;
            SimpleEvent lastEvent = null;

            eventStore.ObserveEvent<AddItemInCartCommand>()
                .Subscribe(@event =>
                {
                    eventListenedCount++;
                    lastEvent = @event;
                });

            commandDispatcher.Dispatch(new AddItemInCartCommand
            {
                ItemName = "Book",
                UnitCost = 45
            });
            commandDispatcher.Dispatch(new ResetCartCommand());

            // Assert
            Assert.Equal(1, eventListenedCount);
            Assert.IsType<AddItemInCartCommand>(lastEvent.Data);
        }
    }
}
