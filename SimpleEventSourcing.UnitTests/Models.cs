using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SimpleEventSourcing.UnitTests
{
    public class CartCommandDispatcher : CommandDispatcher<object, SimpleEvent>
    {
        protected override IEnumerable<SimpleEvent> Convert(object command)
        {
            // Simple Command Sourcing
            return new List<SimpleEvent>
            {
                new SimpleEvent
                {
                    EventName = command.GetType().Name,
                    Data = command,
                    Metadata = new { CreatedDate = DateTime.Now }
                }
            };
        }
    }

    public class CartEventStore : EventStore<SimpleEvent>
    {
        public CartEventStore(IObservable<IEnumerable<SimpleEvent>> eventAggregates) : base(eventAggregates)
        {
        }
    }

    public class TotalCostCartState
    {
        public decimal TotalCost { get; set; }
    }
    public class TotalCostCartEventView : InMemoryEventView<SimpleEvent, TotalCostCartState>
    {
        public TotalCostCartEventView(IObservable<SimpleEvent> events) : base(events)
        {
        }

        protected override TotalCostCartState Reduce(TotalCostCartState state, SimpleEvent @event)
        {
            if (@event.EventName == nameof(AddItemInCartCommand))
            {
                var data = @event.Data as AddItemInCartCommand;
                return new TotalCostCartState
                {
                    TotalCost = state.TotalCost + (data.NumberOfUnits * data.UnitCost)
                };
            }
            if (@event.EventName == nameof(RemoveItemFromCartCommand))
            {
                var data = @event.Data as RemoveItemFromCartCommand;
                return new TotalCostCartState
                {
                    TotalCost = state.TotalCost - data.UnitCost
                };
            }
            if (@event.EventName == nameof(ResetCartCommand))
            {
                return new TotalCostCartState
                {
                    TotalCost = 0
                };
            }
            return state;
        }
    }

    public class OrdersCartState
    {
        public ImmutableDictionary<string, long> Items { get; set; } = ImmutableDictionary<string, long>.Empty;
        public int NumberOfItems { get; set; }
    }
    public class OrdersCartEventView : InMemoryEventView<SimpleEvent, OrdersCartState>
    {
        public OrdersCartEventView(IObservable<SimpleEvent> events) : base(events)
        {
        }

        protected override OrdersCartState Reduce(OrdersCartState state, SimpleEvent @event)
        {
            if (@event.EventName == nameof(AddItemInCartCommand))
            {
                var data = @event.Data as AddItemInCartCommand;

                if (state.Items.ContainsKey(data.ItemName))
                {
                    return new OrdersCartState
                    {
                        Items = state.Items.SetItem(data.ItemName, state.Items[data.ItemName] + data.NumberOfUnits),
                        NumberOfItems = state.NumberOfItems + data.NumberOfUnits
                    };
                }
                else
                {
                    return new OrdersCartState
                    {
                        Items = state.Items.Add(data.ItemName, data.NumberOfUnits),
                        NumberOfItems = state.NumberOfItems + data.NumberOfUnits
                    };
                }
            }
            if (@event.EventName == nameof(RemoveItemFromCartCommand))
            {
                var data = @event.Data as RemoveItemFromCartCommand;

                return new OrdersCartState
                {
                    Items = state.Items.SetItem(data.ItemName, state.Items[data.ItemName] - 1),
                    NumberOfItems = state.NumberOfItems - 1
                };
            }
            if (@event.EventName == nameof(ResetCartCommand))
            {
                return new OrdersCartState
                {
                    Items = ImmutableDictionary<string, long>.Empty,
                    NumberOfItems = 0
                };
            }
            return state;
        }
    }

    public class AddItemInCartCommand
    {
        public string ItemName { get; set; }
        public decimal UnitCost { get; set; }
        public int NumberOfUnits { get; set; } = 1;
    }

    public class RemoveItemFromCartCommand
    {
        public string ItemName { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class ResetCartCommand
    {
    }
}
