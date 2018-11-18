using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static SimpleEventSourcing.Samples.Web.Program;
using static SimpleEventSourcing.Samples.Web.Database.Configuration;
using System.Linq;
using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Dynamic;

namespace SimpleEventSourcing.Samples.Web.Controllers
{
    [Route("api/[controller]")]
    public class EventController : Controller
    {
        private readonly IHubContext<CartHub> _cartHubContext;
        private readonly IHubContext<OrderHub> _orderHubContext;
        private readonly IHubContext<ItemHub> _itemHubContext;

        public EventController(
            IHubContext<CartHub> cartHubContext,
            IHubContext<OrderHub> orderHubContext,
            IHubContext<ItemHub> itemHubContext)
        {
            _cartHubContext = cartHubContext;
            _orderHubContext = orderHubContext;
            _itemHubContext = itemHubContext;
        }

        [HttpGet("all")]
        public IEnumerable<AppEvent> GetAll()
        {
            using (var connection = GetEventsDatabaseConnection())
            {
                return connection
                    .Query<EventDbo>("SELECT * FROM [Event] ORDER BY [Id] DESC")
                    .Select(eventDbo =>
                    {
                        return new AppEvent
                        {
                            Id = eventDbo.Id,
                            EventName = eventDbo.EventName,
                            Data = JsonConvert.DeserializeObject<ExpandoObject>(eventDbo.Data),
                            Metadata = JsonConvert.DeserializeObject<ExpandoObject>(eventDbo.Metadata)
                        };
                    })
                    .ToList();
            }
        }

        [HttpPost("replay")]
        public async Task Replay()
        {
            // Get events stored
            IEnumerable<SimpleEvent> events;
            using (var connection = GetEventsDatabaseConnection())
            {
                events = connection
                    .Query<EventDbo>("SELECT * FROM [Event] ORDER BY [Id] ASC")
                    .Select(eventDbo =>
                    {
                        return new AppEvent
                        {
                            Id = eventDbo.Id,
                            EventName = eventDbo.EventName,
                            Data = JsonConvert.DeserializeObject(eventDbo.Data),
                            Metadata = JsonConvert.DeserializeObject(eventDbo.Metadata)
                        };
                    })
                    .ToList();
            }

            // Clear views database
            using (var connection = GetViewsDatabaseConnection())
            {
                connection.Execute(
                    @"
                    DELETE FROM [ItemOrdered];
                    DELETE FROM [Cart];
                    DELETE FROM [Order];
                    DELETE FROM [Item];
                    DELETE FROM [sqlite_sequence];
                    "
                );
            }

            // Replay events
            foreach (var @event in events)
            {
                CartEventView.Replay(@event);
                ItemEventView.Replay(@event);
                OrderEventView.Replay(@event);
            }

            List<Item> inventoryItems;
            List<ItemAndQuantity> cartItems;
            List<Order> orders;

            // Get results from the replay in realtime
            using (var connection = GetViewsDatabaseConnection())
            {
                inventoryItems = connection
                    .Query<ItemDbo>("SELECT * FROM [Item]")
                    .Select(item =>
                    {
                        return new Item
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Price = Convert.ToDecimal(item.Price),
                            RemainingQuantity = item.RemainingQuantity,
                        };
                    })
                    .ToList();

                cartItems = connection
                    .Query<ItemAndQuantity>("SELECT [ItemId], [Quantity] FROM [Cart]")
                    .ToList();

                var ordersDbo = connection
                    .Query<OrderDbo>("SELECT * FROM [Order]")
                    .ToList();
                var itemsOrderedDbo = connection
                    .Query<ItemOrderedDbo>("SELECT * FROM [ItemOrdered]")
                    .ToList();

                orders = ordersDbo
                    .Select(order =>
                    {
                        return new Order
                        {
                            Id = order.Id,
                            CreatedDate = order.CreatedDate,
                            Number = order.Number,
                            IsConfirmed = order.IsConfirmed,
                            IsCanceled = order.IsCanceled,
                            Items = itemsOrderedDbo
                                .Where(i => i.OrderId == order.Id)
                                .Select(i =>
                                {
                                    return new ItemAndPriceAndQuantity
                                    {
                                        ItemId = i.ItemId,
                                        Price = Convert.ToDecimal(i.Price),
                                        Quantity = i.Quantity
                                    };
                                })
                        };
                    })
                    .ToList();
            }

            // Sync data with the client
            foreach (var item in inventoryItems)
            {
                await _itemHubContext.Clients.All.SendAsync("Sync", item);
            }

            foreach (var cartItem in cartItems)
            {
                await _cartHubContext.Clients.All.SendAsync("Sync", cartItem);
            }

            foreach (var order in orders)
            {
                await _orderHubContext.Clients.All.SendAsync("Sync", order);
            }
        }
    }
}
