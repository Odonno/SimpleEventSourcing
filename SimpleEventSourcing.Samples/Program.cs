using System;
using System.Collections.Immutable;
using System.Linq;

namespace SimpleEventSourcing.Samples
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Create Event Store (Write model) and Event Views (Read model)
            var eventStore = new CartEventStore();
            var totalCostCartEventView = new TotalCostCartEventView();
            var ordersCartEventView = new OrdersCartEventView();

            // Link store events and views
            eventStore.ObserveEvent()
                .Subscribe(@event =>
                {
                    totalCostCartEventView.Handle(@event);
                    ordersCartEventView.Handle(@event);
                });

            // Listen to views changes
            totalCostCartEventView.ObserveState()
                .Subscribe(state =>
                {
                    Console.WriteLine($"Total cost: ${state.TotalCost}");
                });

            ordersCartEventView.ObserveState()
                .Subscribe(state =>
                {
                    if (state.Items.IsEmpty)
                    {
                        Console.WriteLine("Cart: Empty");
                    }
                    else
                    {
                        Console.WriteLine($"Cart: {string.Join(", ", state.Items.Select(item => item.Key + " x" + item.Value))}");
                    }
                });

            // Dispatch events
            eventStore.Dispatch(new AddItemInCartEvent
            {
                ItemName = "Book",
                UnitCost = 45
            });
            eventStore.Dispatch(new AddItemInCartEvent
            {
                ItemName = "Car",
                UnitCost = 14000
            });
            eventStore.Dispatch(new AddItemInCartEvent
            {
                ItemName = "Candy",
                UnitCost = 0.8m,
                NumberOfUnits = 12
            });
            eventStore.Dispatch(new ResetCartEvent());
            eventStore.Dispatch(new AddItemInCartEvent
            {
                ItemName = "Book",
                UnitCost = 30,
                NumberOfUnits = 2
            });
            eventStore.Dispatch(new AddItemInCartEvent
            {
                ItemName = "Book",
                UnitCost = 30,
                NumberOfUnits = 3
            });
            eventStore.Dispatch(new RemoveItemFromCartEvent
            {
                ItemName = "Book",
                UnitCost = 30
            });

            Console.ReadLine();
        }
    }

    public class CartEventStore : EventStore { }

    public class TotalCostCartState
    {
        public decimal TotalCost { get; set; }
    }
    public class TotalCostCartEventView : EventView<TotalCostCartState>
    {
        protected override TotalCostCartState Execute(TotalCostCartState state, object @event)
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
    }
    public class OrdersCartEventView : EventView<OrdersCartState>
    {
        protected override OrdersCartState Execute(OrdersCartState state, object @event)
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
