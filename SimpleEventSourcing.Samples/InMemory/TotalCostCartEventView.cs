using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Samples.InMemory
{
    public class TotalCostCartState
    {
        public decimal TotalCost { get; set; }
    }
    public class TotalCostCartEventView : InMemoryEventView<TotalCostCartState>
    {
        public static Dictionary<string, decimal> CostPerItem = new Dictionary<string, decimal>
        {
            { "Book", 30 },
            { "Car", 14000 },
            { "Candy", 0.8m }
        };

        public TotalCostCartEventView(IObservable<object> events) : base(events)
        {
        }

        protected override TotalCostCartState Reduce(TotalCostCartState state, object @event)
        {
            if (@event is AddItemInCartEvent addItemInCartEvent)
            {
                var unitCost = CostPerItem.GetValueOrDefault(addItemInCartEvent.ItemName);
                return new TotalCostCartState
                {
                    TotalCost = state.TotalCost + (addItemInCartEvent.NumberOfUnits * unitCost)
                };
            }
            if (@event is RemoveItemFromCartEvent removeItemFromCartEvent)
            {
                var unitCost = CostPerItem.GetValueOrDefault(removeItemFromCartEvent.ItemName);
                return new TotalCostCartState
                {
                    TotalCost = state.TotalCost - unitCost
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
}
