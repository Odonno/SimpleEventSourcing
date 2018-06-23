using System;
using Xunit;

namespace SimpleEventSourcing.UnitTests
{
    public class EventStoreTest
    {
        [Fact]
        public void CanDispatchEvent()
        {
            // Arrange
            var eventStore = new CartEventStore();

            // Act
            eventStore.Dispatch(new AddItemInCartEvent
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
            var eventStore = new CartEventStore();

            // Act
            int eventListenedCount = 0;

            eventStore.ObserveEvent()
                .Subscribe(_ =>
                {
                    eventListenedCount++;
                });

            eventStore.Dispatch(new AddItemInCartEvent
            {
                ItemName = "Book",
                UnitCost = 45
            });
            eventStore.Dispatch(new AddItemInCartEvent
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
            var eventStore = new CartEventStore();

            // Act
            int eventListenedCount = 0;
            object lastEvent = null;

            eventStore.ObserveEvent()
                .Subscribe(@event =>
                {
                    eventListenedCount++;
                    lastEvent = @event;
                });

            eventStore.Dispatch(new AddItemInCartEvent
            {
                ItemName = "Book",
                UnitCost = 45
            });
            eventStore.Dispatch(new ResetCartEvent());

            // Assert
            Assert.Equal(2, eventListenedCount);
            Assert.IsType<ResetCartEvent>(lastEvent);
        }

        [Fact]
        public void CanObserveSingleEventType()
        {
            // Arrange
            var eventStore = new CartEventStore();

            // Act
            int eventListenedCount = 0;
            object lastEvent = null;

            eventStore.ObserveEvent<AddItemInCartEvent>()
                .Subscribe(@event =>
                {
                    eventListenedCount++;
                    lastEvent = @event;
                });

            eventStore.Dispatch(new AddItemInCartEvent
            {
                ItemName = "Book",
                UnitCost = 45
            });
            eventStore.Dispatch(new ResetCartEvent());

            // Assert
            Assert.Equal(1, eventListenedCount);
            Assert.IsType<AddItemInCartEvent>(lastEvent);
        }
    }
}
