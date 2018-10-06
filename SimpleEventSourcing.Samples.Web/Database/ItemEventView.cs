using Dapper;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static SimpleEventSourcing.Samples.Web.DatabaseConfiguration;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public class ItemEventView : EventView
    {
        private readonly Subject<Item> _updatedEntitySubject = new Subject<Item>();

        public ItemEventView(IObservable<object> events) : base(events)
        {
        }

        public IObservable<Item> ObserveEntityChange()
        {
            return _updatedEntitySubject.DistinctUntilChanged();
        }

        protected override void Handle(object @event, bool replayed = false)
        {
            if (@event is CreateItemEvent createItemEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    var newItem = connection.Query<ItemDbo>(
                        @"
                        INSERT INTO [Item] 
                        ([Name], [Price], [RemainingQuantity])
                        VALUES (@Name, @Price, @InitialQuantity);

                        SELECT * FROM [Item] ORDER BY [Id] DESC LIMIT 1;
                        ",
                        new { createItemEvent.Name, createItemEvent.Price, createItemEvent.InitialQuantity }
                    )
                    .Single();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Item
                        {
                            Id = newItem.Id,
                            Name = newItem.Name,
                            Price = Convert.ToDecimal(newItem.Price),
                            RemainingQuantity = newItem.RemainingQuantity,
                        });
                    }
                }
            }
            if (@event is UpdateItemPriceEvent updateItemPriceEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    var updatedItem = connection.Query<ItemDbo>(
                        @"
                        UPDATE [Item] 
                        SET [Price] = @NewPrice
                        WHERE [Id] = @ItemId;

                        SELECT * FROM [Item] WHERE [Id] = @ItemId;
                        ",
                        new { updateItemPriceEvent.ItemId, updateItemPriceEvent.NewPrice }
                    )
                    .Single();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Item
                        {
                            Id = updatedItem.Id,
                            Name = updatedItem.Name,
                            Price = Convert.ToDecimal(updatedItem.Price),
                            RemainingQuantity = updatedItem.RemainingQuantity,
                        });
                    }
                }
            }
            if (@event is SupplyItemEvent supplyItemEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    var updatedItem = connection.Query<ItemDbo>(
                        @"
                        UPDATE [Item] 
                        SET [RemainingQuantity] = [RemainingQuantity] + @Quantity
                        WHERE [Id] = @ItemId;

                        SELECT * FROM [Item] WHERE [Id] = @ItemId;
                        ",
                        new { supplyItemEvent.ItemId, supplyItemEvent.Quantity }
                    )
                    .Single();

                    if (!replayed)
                    {
                        _updatedEntitySubject.OnNext(new Item
                        {
                            Id = updatedItem.Id,
                            Name = updatedItem.Name,
                            Price = Convert.ToDecimal(updatedItem.Price),
                            RemainingQuantity = updatedItem.RemainingQuantity,
                        });
                    }
                }
            }
            if (@event is ValidateOrderEvent validateOrderEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    var updatedItems = connection.Query<ItemDbo>(
                        @"
                        UPDATE [Item] 
                        SET [RemainingQuantity] = [RemainingQuantity] - (SELECT [Quantity] FROM [ItemOrdered] WHERE [OrderId] = @OrderId AND [ItemId] = [Item].[Id])
                        WHERE [Id] IN (SELECT [ItemId] FROM [ItemOrdered] WHERE [OrderId] = @OrderId);

                        SELECT * FROM [Item] WHERE [Id] IN (SELECT [ItemId] FROM [ItemOrdered] WHERE [OrderId] = @OrderId);
                        ",
                        new { validateOrderEvent.OrderId }
                    )
                    .ToList();

                    if (!replayed)
                    {
                        foreach (var updatedItem in updatedItems)
                        {
                            _updatedEntitySubject.OnNext(new Item
                            {
                                Id = updatedItem.Id,
                                Name = updatedItem.Name,
                                Price = Convert.ToDecimal(updatedItem.Price),
                                RemainingQuantity = updatedItem.RemainingQuantity,
                            });
                        }
                    }
                }
            }
        }
    }
}
