using Dapper;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static SimpleEventSourcing.Samples.Web.Database.Configuration;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public class ItemEventView : EventView<SimpleEvent>
    {
        private readonly Subject<Item> _updatedEntitySubject = new Subject<Item>();

        public ItemEventView(IObservable<SimpleEvent> events) : base(events)
        {
        }

        public IObservable<Item> ObserveEntityChange()
        {
            return _updatedEntitySubject.DistinctUntilChanged();
        }

        protected override void Handle(SimpleEvent @event, bool replayed = false)
        {
            if (@event.EventName == nameof(ItemRegistered))
            {
                var data = @event.Data as ItemRegistered;

                using (var connection = GetViewsDatabaseConnection())
                {
                    var newItem = connection.Query<ItemDbo>(
                        @"
                        INSERT INTO [Item] 
                        ([Name], [Price], [RemainingQuantity])
                        VALUES (@Name, @Price, @InitialQuantity);

                        SELECT * FROM [Item] ORDER BY [Id] DESC LIMIT 1;
                        ",
                        new { data.Name, data.Price, data.InitialQuantity }
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
            if (@event.EventName == nameof(ItemPriceUpdated))
            {
                var data = @event.Data as ItemPriceUpdated;

                using (var connection = GetViewsDatabaseConnection())
                {
                    var updatedItem = connection.Query<ItemDbo>(
                        @"
                        UPDATE [Item] 
                        SET [Price] = @NewPrice
                        WHERE [Id] = @ItemId;

                        SELECT * FROM [Item] WHERE [Id] = @ItemId;
                        ",
                        new { data.ItemId, data.NewPrice }
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
            if (@event.EventName == nameof(ItemSupplied))
            {
                var data = @event.Data as ItemSupplied;

                using (var connection = GetViewsDatabaseConnection())
                {
                    var updatedItem = connection.Query<ItemDbo>(
                        @"
                        UPDATE [Item] 
                        SET [RemainingQuantity] = [RemainingQuantity] + @Quantity
                        WHERE [Id] = @ItemId;

                        SELECT * FROM [Item] WHERE [Id] = @ItemId;
                        ",
                        new { data.ItemId, data.Quantity }
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
            if (@event.EventName == nameof(OrderValidated))
            {
                var data = @event.Data as OrderValidated;

                using (var connection = GetViewsDatabaseConnection())
                {
                    var updatedItems = connection.Query<ItemDbo>(
                        @"
                        UPDATE [Item] 
                        SET [RemainingQuantity] = [RemainingQuantity] - (SELECT [Quantity] FROM [ItemOrdered] WHERE [OrderId] = @OrderId AND [ItemId] = [Item].[Id])
                        WHERE [Id] IN (SELECT [ItemId] FROM [ItemOrdered] WHERE [OrderId] = @OrderId);

                        SELECT * FROM [Item] WHERE [Id] IN (SELECT [ItemId] FROM [ItemOrdered] WHERE [OrderId] = @OrderId);
                        ",
                        new { data.OrderId }
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
