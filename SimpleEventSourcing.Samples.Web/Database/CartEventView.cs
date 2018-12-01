using Converto;
using Dapper;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static SimpleEventSourcing.Samples.Web.Database.Configuration;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public class CartEventView : EventView<SimpleEvent>
    {
        private readonly Subject<ItemAndQuantity> _updatedEntitySubject = new Subject<ItemAndQuantity>();

        public CartEventView(IObservable<SimpleEvent> events) : base(events)
        {
        }

        public IObservable<ItemAndQuantity> ObserveEntityChange()
        {
            return _updatedEntitySubject.DistinctUntilChanged();
        }

        protected override void Handle(SimpleEvent @event, bool replayed = false)
        {
            if (@event.EventName == nameof(CartItemSelected))
            {
                var data = @event.Data.ConvertTo<CartItemSelected>();

                using (var connection = GetViewsDatabaseConnection())
                {
                    bool canUpdate = connection
                        .Query<int>("SELECT COUNT(*) FROM [Cart] WHERE [ItemId] = @ItemId", new { data.ItemId })
                        .Single() > 0;

                    if (canUpdate)
                    {
                        var newQuantity = connection.Query<int>(
                            @"
                            UPDATE [Cart] 
                            SET [Quantity] = [Quantity] + @Quantity
                            WHERE [ItemId] = @ItemId;
                        
                            SELECT [Quantity] FROM [Cart] WHERE [ItemId] = @ItemId;
                            ",
                            new { data.ItemId, data.Quantity }
                        )
                        .Single();

                        if (!replayed)
                        {
                            _updatedEntitySubject.OnNext(new ItemAndQuantity
                            {
                                ItemId = data.ItemId,
                                Quantity = newQuantity
                            });
                        }
                    }
                    else
                    {
                        connection.Execute(
                            @"
                            INSERT INTO [Cart] 
                            ([ItemId], [Quantity])
                            VALUES (@ItemId, @Quantity)",
                            new { data.ItemId, data.Quantity }
                        );

                        if (!replayed)
                        {
                            _updatedEntitySubject.OnNext(new ItemAndQuantity
                            {
                                ItemId = data.ItemId,
                                Quantity = data.Quantity
                            });
                        }
                    }
                }
            }
            if (@event.EventName == nameof(CartItemUnselected))
            {
                var data = @event.Data.ConvertTo<CartItemUnselected>();

                using (var connection = GetViewsDatabaseConnection())
                {
                    int quantityInCart = connection
                        .Query<int>("SELECT [Quantity] FROM [Cart] WHERE [ItemId] = @ItemId", new { data.ItemId })
                        .Single();
                    bool canDelete = data.Quantity >= quantityInCart;

                    if (canDelete)
                    {
                        connection.Execute("DELETE FROM [Cart] WHERE [ItemId] = @ItemId", new { data.ItemId });

                        if (!replayed)
                        {
                            _updatedEntitySubject.OnNext(new ItemAndQuantity
                            {
                                ItemId = data.ItemId,
                                Quantity = 0
                            });
                        }
                    }
                    else
                    {
                        var newQuantity = connection.Query<int>(
                            @"
                            UPDATE [Cart] 
                            SET [Quantity] = [Quantity] - @Quantity
                            WHERE [ItemId] = @ItemId;
                        
                            SELECT [Quantity] FROM [Cart] WHERE [ItemId] = @ItemId;
                            ",
                            new { data.ItemId, data.Quantity }
                        )
                        .Single();

                        if (!replayed)
                        {
                            _updatedEntitySubject.OnNext(new ItemAndQuantity
                            {
                                ItemId = data.ItemId,
                                Quantity = newQuantity
                            });
                        }
                    }
                }
            }
            if (@event.EventName == nameof(CartReseted))
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    var itemIds = connection
                        .Query<int>("SELECT [ItemId] FROM [Cart]")
                        .ToList();

                    connection.Execute("DELETE FROM [Cart]");

                    if (!replayed)
                    {
                        foreach (int itemId in itemIds)
                        {
                            _updatedEntitySubject.OnNext(new ItemAndQuantity
                            {
                                ItemId = itemId,
                                Quantity = 0
                            });
                        }
                    }
                }
            }
        }
    }
}
