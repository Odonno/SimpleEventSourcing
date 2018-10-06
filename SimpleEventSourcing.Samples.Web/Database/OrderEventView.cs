using Dapper;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static SimpleEventSourcing.Samples.Web.DatabaseConfiguration;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public class OrderEventView : EventView
    {
        private readonly Subject<Order> _updatedEntitySubject = new Subject<Order>();

        public OrderEventView(IObservable<object> events) : base(events)
        {
        }

        public IObservable<Order> ObserveEntityChange()
        {
            return _updatedEntitySubject.DistinctUntilChanged();
        }

        protected override void Handle(object @event, bool replayed = false)
        {
            if (@event is CreateOrderFromCartEvent createOrderFromCartEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    var cart = connection
                        .Query<ItemAndQuantity>("SELECT * FROM [Cart]")
                        .ToList();
                    var itemsToOrder = connection
                        .Query<Item>("SELECT * FROM [Item] WHERE [Id] IN @Ids", new { Ids = cart.Select(c => c.ItemId) })
                        .ToList();

                    var newOrder = connection.Query<OrderDbo>(
                        @"
                        INSERT INTO [Order] 
                        ([Number], [CreatedDate], [IsConfirmed], [IsCanceled])
                        VALUES ((SELECT IFNULL(MAX([Number]), 0) + 1 FROM [Order]), @Date, 0, 0);

                        SELECT * FROM [Order] ORDER BY [Id] DESC LIMIT 1;
                        ",
                        new { createOrderFromCartEvent.Date }
                    )
                    .Single();

                    connection.Execute(
                        @"
                        INSERT INTO [ItemOrdered] 
                        ([OrderId], [ItemId], [Quantity], [Price])
                        VALUES (@OrderId, @ItemId, @Quantity, @Price)
                        ",
                        cart.Select(c =>
                        {
                            return new
                            {
                                OrderId = newOrder.Id,
                                c.ItemId,
                                c.Quantity,
                                itemsToOrder.Single(item => item.Id == c.ItemId).Price
                            };
                        })
                    );

                    var newOrderedItems = connection.Query<ItemOrderedDbo>(
                        "SELECT * FROM [ItemOrdered] WHERE [OrderId] = @OrderId",
                        new { OrderId = newOrder.Id }
                    )
                    .ToList();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Order
                        {
                            Id = newOrder.Id,
                            CreatedDate = newOrder.CreatedDate,
                            Number = newOrder.Number,
                            IsConfirmed = newOrder.IsConfirmed,
                            IsCanceled = newOrder.IsCanceled,
                            Items = newOrderedItems
                                .Select(i =>
                                {
                                    return new ItemAndPriceAndQuantity
                                    {
                                        ItemId = i.ItemId,
                                        Price = Convert.ToDecimal(i.Price),
                                        Quantity = i.Quantity
                                    };
                                })
                        });
                    }
                }
            }
            if (@event is ValidateOrderEvent validateOrderEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    connection.Execute(
                        @"
                        UPDATE [Order] 
                        SET [IsConfirmed] = 1
                        WHERE [Id] = @OrderId",
                        new { validateOrderEvent.OrderId }
                    );

                    var order = connection.Query<OrderDbo>(
                        "SELECT * FROM [Order] WHERE [Id] = @OrderId",
                        new { validateOrderEvent.OrderId }
                    )
                    .Single();

                    var orderedItems = connection.Query<ItemOrderedDbo>(
                        "SELECT * FROM [ItemOrdered] WHERE [OrderId] = @OrderId",
                        new { validateOrderEvent.OrderId }
                    )
                    .ToList();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Order
                        {
                            Id = order.Id,
                            CreatedDate = order.CreatedDate,
                            Number = order.Number,
                            IsConfirmed = order.IsConfirmed,
                            IsCanceled = order.IsCanceled,
                            Items = orderedItems
                                .Select(i =>
                                {
                                    return new ItemAndPriceAndQuantity
                                    {
                                        ItemId = i.ItemId,
                                        Price = Convert.ToDecimal(i.Price),
                                        Quantity = i.Quantity
                                    };
                                })
                        });
                    }
                }
            }
            if (@event is CancelOrderEvent cancelOrderEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    connection.Execute(
                        @"
                        UPDATE [Order] 
                        SET [IsCanceled] = 1
                        WHERE [Id] = @OrderId",
                        new { cancelOrderEvent.OrderId }
                    );

                    var order = connection.Query<OrderDbo>(
                        "SELECT * FROM [Order] WHERE [Id] = @OrderId",
                        new { cancelOrderEvent.OrderId }
                    )
                    .Single();

                    var orderedItems = connection.Query<ItemOrderedDbo>(
                        "SELECT * FROM [ItemOrdered] WHERE [OrderId] = @OrderId",
                        new { cancelOrderEvent.OrderId }
                    )
                    .ToList();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Order
                        {
                            Id = order.Id,
                            CreatedDate = order.CreatedDate,
                            Number = order.Number,
                            IsConfirmed = order.IsConfirmed,
                            IsCanceled = order.IsCanceled,
                            Items = orderedItems
                                .Select(i =>
                                {
                                    return new ItemAndPriceAndQuantity
                                    {
                                        ItemId = i.ItemId,
                                        Price = Convert.ToDecimal(i.Price),
                                        Quantity = i.Quantity
                                    };
                                })
                        });
                    }
                }
            }
        }
    }
}
