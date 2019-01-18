using Converto;
using Dapper;
using Newtonsoft.Json;
using SimpleEventSourcing.Samples.EventStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static SimpleEventSourcing.Samples.Orders.Configuration;

namespace SimpleEventSourcing.Samples.Orders
{
    public class OrderEventView : EventView<SimpleEvent>
    {
        private readonly Subject<Order> _updatedEntitySubject = new Subject<Order>();

        public OrderEventView(IObservable<SimpleEvent> events) : base(events)
        {
        }

        public IObservable<Order> ObserveEntityChange()
        {
            return _updatedEntitySubject.DistinctUntilChanged();
        }

        protected override void Handle(SimpleEvent @event, bool replayed = false)
        {
            if (@event.EventName == nameof(OrderedFromCart))
            {
                var data = @event.Data.ConvertTo<OrderedFromCart>();
                var metadata = @event.Metadata.ConvertTo<SimpleEventMetadata>();

                using (var connection = GetDatabaseConnection())
                {
                    //var cart = connection
                    //    .Query<ItemAndQuantity>("SELECT * FROM [Cart]")
                    //    .ToList();
                    //var itemsToOrder = connection
                    //    .Query<Item>("SELECT * FROM [Item] WHERE [Id] IN @Ids", new { Ids = cart.Select(c => c.ItemId) })
                    //    .ToList();

                    var newOrder = connection.Query<OrderDbo>(
                        @"
                        INSERT INTO [Order] 
                        ([Number], [CreatedDate], [IsConfirmed], [IsCanceled], [Items])
                        VALUES ((SELECT IFNULL(MAX([Number]), 0) + 1 FROM [Order]), @CreatedDate, 0, 0, @Items);

                        SELECT * FROM [Order] ORDER BY [Id] DESC LIMIT 1;
                        ",
                        new {
                            metadata.CreatedDate,
                            Items = JsonConvert.SerializeObject(new List<OrderedItem>()) // TODO
                        }
                    )
                    .Single();

                    //connection.Execute(
                    //    @"
                    //    INSERT INTO [ItemOrdered] 
                    //    ([OrderId], [ItemId], [Quantity], [Price])
                    //    VALUES (@OrderId, @ItemId, @Quantity, @Price)
                    //    ",
                    //    cart.Select(c =>
                    //    {
                    //        return new
                    //        {
                    //            OrderId = newOrder.Id,
                    //            c.ItemId,
                    //            c.Quantity,
                    //            itemsToOrder.Single(item => item.Id == c.ItemId).Price
                    //        };
                    //    })
                    //);

                    //var newOrderedItems = connection.Query<ItemOrderedDbo>(
                    //    "SELECT * FROM [ItemOrdered] WHERE [OrderId] = @OrderId",
                    //    new { OrderId = newOrder.Id }
                    //)
                    //.ToList();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Order
                        {
                            Id = newOrder.Id,
                            CreatedDate = newOrder.CreatedDate,
                            Number = newOrder.Number,
                            IsConfirmed = newOrder.IsConfirmed,
                            IsCanceled = newOrder.IsCanceled,
                            Items = JsonConvert.DeserializeObject<IEnumerable<OrderedItem>>(newOrder.Items)
                            //Items = newOrderedItems
                            //    .Select(i =>
                            //    {
                            //        return new ItemAndPriceAndQuantity
                            //        {
                            //            ItemId = i.ItemId,
                            //            Price = Convert.ToDecimal(i.Price),
                            //            Quantity = i.Quantity
                            //        };
                            //    })
                        });
                    }
                }
            }
            if (@event.EventName == nameof(OrderValidated))
            {
                var data = @event.Data.ConvertTo<OrderValidated>();

                using (var connection = GetDatabaseConnection())
                {
                    connection.Execute(
                        @"
                        UPDATE [Order] 
                        SET [IsConfirmed] = 1
                        WHERE [Id] = @OrderId",
                        new { data.OrderId }
                    );

                    var order = connection.Query<OrderDbo>(
                        "SELECT * FROM [Order] WHERE [Id] = @OrderId",
                        new { data.OrderId }
                    )
                    .Single();

                    //var orderedItems = connection.Query<ItemOrderedDbo>(
                    //    "SELECT * FROM [ItemOrdered] WHERE [OrderId] = @OrderId",
                    //    new { data.OrderId }
                    //)
                    //.ToList();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Order
                        {
                            Id = order.Id,
                            CreatedDate = order.CreatedDate,
                            Number = order.Number,
                            IsConfirmed = order.IsConfirmed,
                            IsCanceled = order.IsCanceled,
                            Items = JsonConvert.DeserializeObject<IEnumerable<OrderedItem>>(order.Items)
                            //Items = orderedItems
                            //    .Select(i =>
                            //    {
                            //        return new ItemAndPriceAndQuantity
                            //        {
                            //            ItemId = i.ItemId,
                            //            Price = Convert.ToDecimal(i.Price),
                            //            Quantity = i.Quantity
                            //        };
                            //    })
                        });
                    }
                }
            }
            if (@event.EventName == nameof(OrderCanceled))
            {
                var data = @event.Data.ConvertTo<OrderCanceled>();

                using (var connection = GetDatabaseConnection())
                {
                    connection.Execute(
                        @"
                        UPDATE [Order] 
                        SET [IsCanceled] = 1
                        WHERE [Id] = @OrderId",
                        new { data.OrderId }
                    );

                    var order = connection.Query<OrderDbo>(
                        "SELECT * FROM [Order] WHERE [Id] = @OrderId",
                        new { data.OrderId }
                    )
                    .Single();

                    //var orderedItems = connection.Query<ItemOrderedDbo>(
                    //    "SELECT * FROM [ItemOrdered] WHERE [OrderId] = @OrderId",
                    //    new { data.OrderId }
                    //)
                    //.ToList();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Order
                        {
                            Id = order.Id,
                            CreatedDate = order.CreatedDate,
                            Number = order.Number,
                            IsConfirmed = order.IsConfirmed,
                            IsCanceled = order.IsCanceled,
                            Items = JsonConvert.DeserializeObject<IEnumerable<OrderedItem>>(order.Items)
                            //Items = orderedItems
                            //    .Select(i =>
                            //    {
                            //        return new ItemAndPriceAndQuantity
                            //        {
                            //            ItemId = i.ItemId,
                            //            Price = Convert.ToDecimal(i.Price),
                            //            Quantity = i.Quantity
                            //        };
                            //    })
                        });
                    }
                }
            }
        }
    }
}
