using System;
using System.Collections.Immutable;

namespace SimpleEventSourcing.UnitTests
{
    public class CartEventStore : EventStore { }

    public class TotalCostCartState
    {
        public decimal TotalCost { get; set; }
    }
    public class TotalCostCartEventView : EventView<TotalCostCartState>
    {
        public TotalCostCartEventView(IObservable<object> events) : base(events)
        {
        }

        protected override TotalCostCartState Reduce(TotalCostCartState state, object @event)
        {
            if (@event is AddItemInCartEvent addItemInCartEvent)
            {
                return new TotalCostCartState
                {
                    TotalCost = state.TotalCost + (addItemInCartEvent.NumberOfUnits * addItemInCartEvent.UnitCost)
                };
            }
            if (@event is RemoveItemFromCartEvent removeItemFromCartEvent)
            {
                return new TotalCostCartState
                {
                    TotalCost = state.TotalCost - removeItemFromCartEvent.UnitCost
                };
            }
            if (@event is ResetCartEvent)
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
    public class OrdersCartEventView : EventView<OrdersCartState>
    {
        public OrdersCartEventView(IObservable<object> events) : base(events)
        {
        }

        protected override OrdersCartState Reduce(OrdersCartState state, object @event)
        {
            if (@event is AddItemInCartEvent addItemInCartEvent)
            {
                if (state.Items.ContainsKey(addItemInCartEvent.ItemName))
                {
                    return new OrdersCartState
                    {
                        Items = state.Items.SetItem(addItemInCartEvent.ItemName, state.Items[addItemInCartEvent.ItemName] + addItemInCartEvent.NumberOfUnits),
                        NumberOfItems = state.NumberOfItems + addItemInCartEvent.NumberOfUnits
                    };
                }
                else
                {
                    return new OrdersCartState
                    {
                        Items = state.Items.Add(addItemInCartEvent.ItemName, addItemInCartEvent.NumberOfUnits),
                        NumberOfItems = state.NumberOfItems + addItemInCartEvent.NumberOfUnits
                    };
                }
            }
            if (@event is RemoveItemFromCartEvent removeItemFromCartEvent)
            {
                return new OrdersCartState
                {
                    Items = state.Items.SetItem(removeItemFromCartEvent.ItemName, state.Items[removeItemFromCartEvent.ItemName] - 1),
                    NumberOfItems = state.NumberOfItems - 1
                };
            }
            if (@event is ResetCartEvent)
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

    public class AddItemInCartEvent
    {
        public string ItemName { get; set; }
        public decimal UnitCost { get; set; }
        public int NumberOfUnits { get; set; } = 1;
    }

    public class RemoveItemFromCartEvent
    {
        public string ItemName { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class ResetCartEvent
    {
    }
}
