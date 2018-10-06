using Dapper;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static SimpleEventSourcing.Samples.Web.DatabaseConfiguration;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public class CartEventView : EventView
    {
        private readonly Subject<ItemAndQuantity> _updatedEntitySubject = new Subject<ItemAndQuantity>();

        public CartEventView(IObservable<object> events) : base(events)
        {
        }

        public IObservable<ItemAndQuantity> ObserveEntityChange()
        {
            return _updatedEntitySubject.DistinctUntilChanged();
        }

        protected override void Handle(object @event, bool replayed = false)
        {
            if (@event is AddItemInCartEvent addItemInCartEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    bool canUpdate = connection
                        .Query<int>("SELECT COUNT(*) FROM [Cart] WHERE [ItemId] = @ItemId", new { addItemInCartEvent.ItemId })
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
                            new { addItemInCartEvent.ItemId, addItemInCartEvent.Quantity }
                        )
                        .Single();

                        if (!replayed)
                        {
                            _updatedEntitySubject.OnNext(new ItemAndQuantity
                            {
                                ItemId = addItemInCartEvent.ItemId,
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
                            new { addItemInCartEvent.ItemId, addItemInCartEvent.Quantity }
                        );

                        if (!replayed)
                        {
                            _updatedEntitySubject.OnNext(new ItemAndQuantity
                            {
                                ItemId = addItemInCartEvent.ItemId,
                                Quantity = addItemInCartEvent.Quantity
                            });
                        }
                    }
                }
            }
            if (@event is RemoveItemFromCartEvent removeItemFromCartEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    int quantityInCart = connection
                        .Query<int>("SELECT [Quantity] FROM [Cart] WHERE [ItemId] = @ItemId", new { removeItemFromCartEvent.ItemId })
                        .Single();
                    bool canDelete = removeItemFromCartEvent.Quantity >= quantityInCart;

                    if (canDelete)
                    {
                        connection.Execute("DELETE FROM [Cart] WHERE [ItemId] = @ItemId", new { removeItemFromCartEvent.ItemId });

                        if (!replayed)
                        {
                            _updatedEntitySubject.OnNext(new ItemAndQuantity
                            {
                                ItemId = removeItemFromCartEvent.ItemId,
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
                            new { removeItemFromCartEvent.ItemId, removeItemFromCartEvent.Quantity }
                        )
                        .Single();

                        if (!replayed)
                        {
                            _updatedEntitySubject.OnNext(new ItemAndQuantity
                            {
                                ItemId = removeItemFromCartEvent.ItemId,
                                Quantity = newQuantity
                            });
                        }
                    }
                }
            }
            if (@event is ResetCartEvent)
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
