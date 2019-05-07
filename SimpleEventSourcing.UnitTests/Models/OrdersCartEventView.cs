using System.Collections.Immutable;

namespace SimpleEventSourcing.UnitTests.Models
{
    public class OrdersCartState
    {
        public ImmutableDictionary<string, long> Items { get; set; } = ImmutableDictionary<string, long>.Empty;
        public int NumberOfItems { get; set; }
    }
    public class OrdersCartEventView : InMemoryEventView<StreamedEvent, OrdersCartState>
    {
        public OrdersCartEventView(IEventStreamProvider<StreamedEvent> streamProvider, OrdersCartState initialState = null) : base(streamProvider, initialState)
        {
        }

        protected override OrdersCartState Reduce(OrdersCartState state, StreamedEvent @event)
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
}
