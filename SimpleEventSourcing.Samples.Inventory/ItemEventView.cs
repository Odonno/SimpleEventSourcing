using Converto;
using Dapper;
using SimpleEventSourcing.Samples.Events;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static SimpleEventSourcing.Samples.Inventory.Configuration;

namespace SimpleEventSourcing.Samples.Inventory
{
    public class ItemEventView : EventView<StreamedEvent>
    {
        private readonly Subject<Item> _updatedEntitySubject = new Subject<Item>();
        
        public ItemEventView(IEventStreamProvider<StreamedEvent> streamProvider) : base(streamProvider)
        {
            // TODO : Extract method
            // Detect new streams
            if (_streamProvider is IRealtimeEventStreamProvider<StreamedEvent> realtimeStreamProvider)
            {
                realtimeStreamProvider.DetectNewStreams().Subscribe(stream =>
                {
                    if (!stream.Id.StartsWith("item-"))
                        return;

                    if (stream is IRealtimeEventStream<StreamedEvent> realtimeItemStream)
                    {
                        realtimeItemStream.ListenForNewEvents(true).Subscribe(@event =>
                        {
                            Handle(@event);
                        });
                    }
                });
            }
        }

        public IObservable<Item> ObserveEntityChange()
        {
            return _updatedEntitySubject.DistinctUntilChanged();
        }

        protected override void Handle(StreamedEvent @event, bool replayed = false)
        {
            if (@event.EventName == nameof(ItemRegistered))
            {
                var data = @event.Data.ConvertTo<ItemRegistered>();

                using (var connection = GetDatabaseConnection())
                {
                    var newItem = connection.Query<ItemDbo>(
                        @"
                        INSERT INTO [Item] 
                        ([Id], [Name], [Price], [RemainingQuantity])
                        VALUES (@Id, @Name, @Price, @InitialQuantity);

                        SELECT * FROM [Item] WHERE [Id] = @Id;
                        ",
                        new { data.Id, data.Name, data.Price, data.InitialQuantity }
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
                var data = @event.Data.ConvertTo<ItemPriceUpdated>();

                using (var connection = GetDatabaseConnection())
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
                var data = @event.Data.ConvertTo<ItemSupplied>();

                using (var connection = GetDatabaseConnection())
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
                var data = @event.Data.ConvertTo<OrderValidated>();

                using (var connection = GetDatabaseConnection())
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
