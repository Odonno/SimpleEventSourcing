using System;
using System.Linq;
using static SimpleEventSourcing.Samples.Database.Functions;

namespace SimpleEventSourcing.Samples
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("1 - Play & Store events in-memory");
                Console.WriteLine("2 - Clear the SQLite database");
                Console.WriteLine("3 - Play & Store events in a SQLite database (v1)");
                Console.WriteLine("4 - Play & Store events in a SQLite database (v2)");
                Console.WriteLine("5 - Exit application");

                Console.WriteLine("Please select a choice / number: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    // Create Event Store (Write model) and Event Views (Read model)
                    var eventStore = new InMemory.CartEventStore();
                    var eventsObservable = eventStore.ObserveEvent(); // get event stream to link event store and views

                    var totalCostCartEventView = new InMemory.TotalCostCartEventView(eventsObservable);
                    var ordersCartEventView = new InMemory.OrdersCartEventView(eventsObservable);

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
                        ItemName = "Book"
                    });
                    eventStore.Dispatch(new AddItemInCartEvent
                    {
                        ItemName = "Car"
                    });
                    eventStore.Dispatch(new AddItemInCartEvent
                    {
                        ItemName = "Candy",
                        NumberOfUnits = 12
                    });
                    eventStore.Dispatch(new ResetCartEvent());
                    eventStore.Dispatch(new AddItemInCartEvent
                    {
                        ItemName = "Book",
                        NumberOfUnits = 2
                    });
                    eventStore.Dispatch(new AddItemInCartEvent
                    {
                        ItemName = "Book",
                        NumberOfUnits = 3
                    });
                    eventStore.Dispatch(new RemoveItemFromCartEvent
                    {
                        ItemName = "Book"
                    });
                }
                if (choice == "2")
                {
                    // Remove Events database
                    RemoveEventsDatabase();

                    // Remove Views database
                    RemoveViewsDatabase();
                }
                if (choice == "3")
                {
                    // Create Events database (if not exists)
                    CreateEventsDatabase();

                    // Create Views database (if not exists and different)
                    CreateViewsDatabase(1);

                    var events = GetEventsFromDatabase();

                    // Create Event Store (Write model) and Event Views (Read model)
                    var eventStore = new Database.CartEventStore();
                    var eventsObservable = eventStore.ObserveEvent(); // get event stream to link event store and views

                    var cartTableEventView = new Database.Version1.CartTableEventView(eventsObservable);

                    if (events.Any())
                    {
                        // TODO : Clear Views database and replay Events (if any in database)
                    }
                    else
                    {
                        // Dispatch new events
                        eventStore.Dispatch(new AddItemInCartEvent
                        {
                            ItemName = "Book"
                        });
                        eventStore.Dispatch(new AddItemInCartEvent
                        {
                            ItemName = "Car"
                        });
                        eventStore.Dispatch(new AddItemInCartEvent
                        {
                            ItemName = "Candy",
                            NumberOfUnits = 12
                        });
                        eventStore.Dispatch(new ResetCartEvent());
                        eventStore.Dispatch(new AddItemInCartEvent
                        {
                            ItemName = "Book",
                            NumberOfUnits = 2
                        });
                        eventStore.Dispatch(new AddItemInCartEvent
                        {
                            ItemName = "Book",
                            NumberOfUnits = 3
                        });
                        eventStore.Dispatch(new RemoveItemFromCartEvent
                        {
                            ItemName = "Book"
                        });
                    }

                    // Get data from read model after all events are dispatched
                    decimal totalCost = GetTotalCostInCart();
                    Console.WriteLine($"Total cost: ${totalCost}");

                    var cartRows = GetCart();
                    if (cartRows.Any())
                    {
                        Console.WriteLine($"Cart: {string.Join(", ", cartRows.Select(item => item.ItemName + " x" + item.NumberOfUnits))}");
                    }
                    else
                    {
                        Console.WriteLine("Cart: Empty");
                    }
                }
                if (choice == "4")
                {
                    // Create Events database (if not exists)
                    CreateEventsDatabase();

                    // Create Views database (if not exists and different)
                    CreateViewsDatabase(2);

                    var events = GetEventsFromDatabase();

                    // Create Event Store (Write model) and Event Views (Read model)
                    var eventStore = new Database.CartEventStore();
                    var eventsObservable = eventStore.ObserveEvent(); // get event stream to link event store and views

                    var cartTableEventView = new Database.Version2.CartTableEventView(eventsObservable);

                    if (events.Any())
                    {
                        // TODO : Clear Views database and replay Events (if any in database)
                    }
                    else
                    {
                        // Dispatch new events
                        eventStore.Dispatch(new AddItemInCartEvent
                        {
                            ItemName = "Book"
                        });
                        eventStore.Dispatch(new AddItemInCartEvent
                        {
                            ItemName = "Car"
                        });
                        eventStore.Dispatch(new AddItemInCartEvent
                        {
                            ItemName = "Candy",
                            NumberOfUnits = 12
                        });
                        eventStore.Dispatch(new ResetCartEvent());
                        eventStore.Dispatch(new AddItemInCartEvent
                        {
                            ItemName = "Book",
                            NumberOfUnits = 2
                        });
                        eventStore.Dispatch(new AddItemInCartEvent
                        {
                            ItemName = "Book",
                            NumberOfUnits = 3
                        });
                        eventStore.Dispatch(new RemoveItemFromCartEvent
                        {
                            ItemName = "Book"
                        });
                    }

                    // Get data from read model after all events are dispatched
                    decimal totalCost = GetTotalCostInCart();
                    Console.WriteLine($"Total cost: ${totalCost}");

                    var cartRows = GetCart();
                    if (cartRows.Any())
                    {
                        Console.WriteLine($"Cart: {string.Join(", ", cartRows.Select(item => item.ItemName + " x" + item.NumberOfUnits))}");
                    }
                    else
                    {
                        Console.WriteLine("Cart: Empty");
                    }
                }
                if (choice == "5")
                {
                    Environment.Exit(0);
                }

                Console.WriteLine(string.Empty);
                Console.WriteLine("---");
            }
        }
    }
}
