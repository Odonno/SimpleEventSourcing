using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Xunit;

namespace SimpleEventSourcing.UnitTests
{
    public class EventViewTest
    {
        [Fact]
        public void CanReadInitialState()
        {
            // Arrange
            var eventSubject = new Subject<object>();
            var eventView = new TotalCostCartEventView(eventSubject.AsObservable());

            // Act

            // Assert
            Assert.Equal(0, eventView.State.TotalCost);
        }

        [Fact]
        public void CanObserveStateWithEventsOfTheSameType()
        {
            // Arrange
            var eventSubject = new Subject<object>();
            var eventView = new TotalCostCartEventView(eventSubject.AsObservable());

            // Act
            int eventListenedCount = 0;
            TotalCostCartState lastState = null;

            eventView.ObserveState()
                .Subscribe(state =>
                {
                    eventListenedCount++;
                    lastState = state;
                });

            eventSubject.OnNext(new AddItemInCartEvent
            {
                ItemName = "Book",
                UnitCost = 45
            });
            eventSubject.OnNext(new AddItemInCartEvent
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
        public void CanObserveStateWithEventsOfDifferentTypes()
        {
            // Arrange
            var eventSubject = new Subject<object>();
            var eventView = new TotalCostCartEventView(eventSubject.AsObservable());

            // Act
            int eventListenedCount = 0;
            TotalCostCartState lastState = null;

            eventView.ObserveState()
                .Subscribe(state =>
                {
                    eventListenedCount++;
                    lastState = state;
                });

            eventSubject.OnNext(new AddItemInCartEvent
            {
                ItemName = "Book",
                UnitCost = 45
            });
            eventSubject.OnNext(new ResetCartEvent());

            // Assert
            Assert.Equal(2, eventListenedCount);
            Assert.NotNull(lastState);
            Assert.Equal(0, lastState.TotalCost);
        }

        [Fact]
        public void CanObserveStateOfMultipleEventViews()
        {
            // Arrange
            var eventSubject = new Subject<object>();
            var totalCostCartEventView = new TotalCostCartEventView(eventSubject.AsObservable());
            var ordersCartEventView = new OrdersCartEventView(eventSubject.AsObservable());

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

            eventSubject.OnNext(new AddItemInCartEvent
            {
                ItemName = "Book",
                UnitCost = 45
            });
            eventSubject.OnNext(new AddItemInCartEvent
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
        public void CanObserveStatePartially()
        {
            // Arrange
            var eventSubject = new Subject<object>();
            var ordersCartEventView = new OrdersCartEventView(eventSubject.AsObservable());

            // Act
            int lastNumberOfItems = 0;

            ordersCartEventView.ObserveState(state => state.NumberOfItems)
                .Subscribe(numberOfItems =>
                {
                    lastNumberOfItems = numberOfItems;
                });

            eventSubject.OnNext(new AddItemInCartEvent
            {
                ItemName = "Book",
                UnitCost = 45
            });
            eventSubject.OnNext(new AddItemInCartEvent
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
