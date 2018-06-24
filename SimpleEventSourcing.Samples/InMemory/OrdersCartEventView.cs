using System;
using System.Collections.Immutable;

namespace SimpleEventSourcing.Samples.InMemory
{
    public class OrdersCartState
    {
        public ImmutableDictionary<string, long> Items { get; set; } = ImmutableDictionary<string, long>.Empty;
    }
    public class OrdersCartEventView : InMemoryEventView<OrdersCartState>
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
                        Items = state.Items.SetItem(addItemInCartEvent.ItemName, state.Items[addItemInCartEvent.ItemName] + addItemInCartEvent.NumberOfUnits)
                    };
                }
                else
                {
                    return new OrdersCartState
                    {
                        Items = state.Items.Add(addItemInCartEvent.ItemName, addItemInCartEvent.NumberOfUnits)
                    };
                }
            }
            if (@event is RemoveItemFromCartEvent removeItemFromCartEvent)
            {
                return new OrdersCartState
                {
                    Items = state.Items.SetItem(removeItemFromCartEvent.ItemName, state.Items[removeItemFromCartEvent.ItemName] - 1)
                };
            }
            if (@event is ResetCartEvent)
            {
                return new OrdersCartState
                {
                    Items = ImmutableDictionary<string, long>.Empty
                };
            }
            return state;
        }
    }
}
