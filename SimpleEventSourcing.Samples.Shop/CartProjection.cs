using Converto;
using Dapper;
using SimpleEventSourcing.Samples.Events;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using static SimpleEventSourcing.Samples.Shop.Configuration;

namespace SimpleEventSourcing.Samples.Shop
{
    public class CartProjection : Projection<StreamedEvent>
    {
        private readonly Subject<Cart> _updatedEntitySubject = new Subject<Cart>();

        public CartProjection(IEventStreamProvider<StreamedEvent> streamProvider) : base(streamProvider)
        {
            const string searchedStream = "cart";

            // Detect new events from existing streams
            var newEventsFromExistingStreams = streamProvider
                .GetAllStreamsAsync()
                .ToObservable()
                .SelectMany(stream => stream)
                .Where(stream => stream.Id == searchedStream)
                .SelectMany(stream => stream.ListenForNewEvents(false));

            // Detect new events from new streams
            var newEventsFromNewStreams = streamProvider
                .ListenForNewStreams()
                .Where(stream => stream.Id == searchedStream)
                .SelectMany(stream => stream.ListenForNewEvents(true));

            // Handle new events
            Observable.Merge(newEventsFromExistingStreams, newEventsFromNewStreams)
                .Subscribe(@event => Handle(@event));
        }

        public IObservable<Cart> ObserveEntityChange()
        {
            return _updatedEntitySubject.DistinctUntilChanged();
        }

        protected override void Handle(StreamedEvent @event, bool replayed = false)
        {
            if (@event.EventName == nameof(CartItemSelected))
            {
                var data = @event.Data.ConvertTo<CartItemSelected>();

                using (var connection = GetDatabaseConnection())
                {
                    bool canUpdate = connection
                        .Query<int>("SELECT COUNT(*) FROM [Cart] WHERE [ItemId] = @ItemId", new { data.ItemId })
                        .Single() > 0;

                    if (canUpdate)
                    {
                        connection.Execute(
                            @"
                            UPDATE [Cart] 
                            SET [Quantity] = [Quantity] + @Quantity
                            WHERE [ItemId] = @ItemId
                            ",
                            new { data.ItemId, data.Quantity }
                        );
                    }
                    else
                    {
                        connection.Execute(
                            @"
                            INSERT INTO [Cart] 
                            ([ItemId], [Quantity])
                            VALUES (@ItemId, @Quantity)
                            ",
                            new { data.ItemId, data.Quantity }
                        );
                    }

                    if (!replayed)
                    {
                        CartUpdated();
                    }
                }
            }
            if (@event.EventName == nameof(CartItemUnselected))
            {
                var data = @event.Data.ConvertTo<CartItemUnselected>();

                using (var connection = GetDatabaseConnection())
                {
                    int quantityInCart = connection
                        .Query<int>("SELECT [Quantity] FROM [Cart] WHERE [ItemId] = @ItemId", new { data.ItemId })
                        .Single();
                    bool canDelete = data.Quantity >= quantityInCart;

                    if (canDelete)
                    {
                        connection.Execute("DELETE FROM [Cart] WHERE [ItemId] = @ItemId", new { data.ItemId });
                    }
                    else
                    {
                        connection.Execute(
                            @"
                            UPDATE [Cart] 
                            SET [Quantity] = [Quantity] - @Quantity
                            WHERE [ItemId] = @ItemId
                            ",
                            new { data.ItemId, data.Quantity }
                        );
                    }

                    if (!replayed)
                    {
                        CartUpdated();
                    }
                }
            }
            if (@event.EventName == nameof(CartReseted) || @event.EventName == nameof(OrderedFromCart))
            {
                using (var connection = GetDatabaseConnection())
                {
                    connection.Execute("DELETE FROM [Cart]");

                    if (!replayed)
                    {
                        CartUpdated();
                    }
                }
            }
        }

        private void CartUpdated()
        {
            using (var connection = GetDatabaseConnection())
            {
                var cart = new Cart
                {
                    Items = connection
                        .Query<ItemAndQuantity>("SELECT [ItemId], [Quantity] FROM [Cart]")
                        .ToList()
                };

                _updatedEntitySubject.OnNext(cart);
            }
        }
    }
}