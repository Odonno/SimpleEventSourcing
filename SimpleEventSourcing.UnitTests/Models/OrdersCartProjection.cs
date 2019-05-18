using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace SimpleEventSourcing.UnitTests.Models
{
    public class OrdersCartState
    {
        public ImmutableDictionary<string, long> Items { get; set; } = ImmutableDictionary<string, long>.Empty;
        public int NumberOfItems { get; set; }
    }
    public class OrdersCartProjection : EntityProjection<StreamedEvent, OrdersCartState>
    {
        public OrdersCartProjection(IEventStreamProvider<StreamedEvent> streamProvider, OrdersCartState initialState = null) : base(streamProvider, initialState)
        {
            _streamProvider.GetStreamAsync("cart")
                .ToObservable()
                .SelectMany(stream => stream.ListenForNewEvents(false))
                .Subscribe(Handle);
        }
        
        protected override OrdersCartState Reduce(OrdersCartState state, StreamedEvent @event)
        {
            if (@event.EventName == nameof(ItemAddedInCart))
            {
                var data = @event.Data as ItemAddedInCart;

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
            if (@event.EventName == nameof(ItemRemovedFromCart))
            {
                var data = @event.Data as ItemRemovedFromCart;

                return new OrdersCartState
                {
                    Items = state.Items.SetItem(data.ItemName, state.Items[data.ItemName] - 1),
                    NumberOfItems = state.NumberOfItems - data.NumberOfUnits
                };
            }
            if (@event.EventName == nameof(CartReset))
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
}
