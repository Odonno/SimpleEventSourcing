using Converto;
using Dapper;
using SimpleEventSourcing.Samples.Events;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using static SimpleEventSourcing.Samples.Inventory.Configuration;

namespace SimpleEventSourcing.Samples.Inventory
{
    public class ItemProjection : Projection<StreamedEvent>
    {
        private readonly Subject<Item> _updatedEntitySubject = new Subject<Item>();
        
        public ItemProjection(IEventStreamProvider<StreamedEvent> streamProvider) : base(streamProvider)
        {
            const string searchedStreams = "item-";

            // Detect new events from existing streams
            var newEventsFromExistingStreams = streamProvider
                .GetAllStreamsAsync()
                .ToObservable()
                .SelectMany(stream => stream)
                .Where(stream => stream.Id.StartsWith(searchedStreams))
                .SelectMany(stream => stream.ListenForNewEvents(false));

            // Detect new events from new streams
            var newEventsFromNewStreams = streamProvider
                .ListenForNewStreams()
                .Where(stream => stream.Id.StartsWith(searchedStreams))
                .SelectMany(stream => stream.ListenForNewEvents(true));

            // Handle new events
            Observable.Merge(newEventsFromExistingStreams, newEventsFromNewStreams)
                .Subscribe(@event => Handle(@event));
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
            if (@event.EventName == nameof(ItemShipped))
            {
                var data = @event.Data.ConvertTo<ItemShipped>();

                using (var connection = GetDatabaseConnection())
                {
                    var updatedItems = connection.Query<ItemDbo>(
                        @"
                        UPDATE [Item] 
                        SET [RemainingQuantity] = [RemainingQuantity] - @Quantity
                        WHERE [Id] = @ItemId;

                        SELECT * FROM [Item] WHERE [Id] = @ItemId;
                        ",
                        new { data.ItemId, data.Quantity }
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
