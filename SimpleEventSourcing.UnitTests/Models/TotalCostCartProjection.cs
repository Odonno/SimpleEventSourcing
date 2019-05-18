using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UnitTests.Models
{
    public class TotalCostCartState
    {
        public decimal TotalCost { get; set; }
        public int NumberOfItems { get; set; }
    }
    public class TotalCostCartProjection : EntityProjection<StreamedEvent, TotalCostCartState>
    {
        public TotalCostCartProjection(IEventStreamProvider<StreamedEvent> streamProvider, TotalCostCartState initialState = null) : base(streamProvider, initialState)
        {
            _streamProvider.GetStreamAsync("cart")
                .ToObservable()
                .SelectMany(stream => stream.ListenForNewEvents(false))
                .Subscribe(Handle);
        }

        protected override TotalCostCartState Reduce(TotalCostCartState state, StreamedEvent @event)
        {
            if (@event.EventName == nameof(ItemAddedInCart))
            {
                var data = @event.Data as ItemAddedInCart;
                return new TotalCostCartState
                {
                    TotalCost = state.TotalCost + (data.NumberOfUnits * data.UnitCost),
                    NumberOfItems = state.NumberOfItems + data.NumberOfUnits
                };
            }
            if (@event.EventName == nameof(ItemRemovedFromCart))
            {
                var data = @event.Data as ItemRemovedFromCart;

                return new TotalCostCartState
                {
                    TotalCost = state.TotalCost * (data.NumberOfUnits / state.NumberOfItems),
                    NumberOfItems = state.NumberOfItems - data.NumberOfUnits
                };
            }
            if (@event.EventName == nameof(CartReset))
            {
                return new TotalCostCartState
                {
                    TotalCost = 0,
                    NumberOfItems = 0
                };
            }
            return state;
        }
    }
}
