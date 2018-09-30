using Dapper;
using System;
using System.Linq;
using static SimpleEventSourcing.Samples.Web.DatabaseConfiguration;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public class CartEventView : EventView
    {
        public CartEventView(IObservable<object> events) : base(events)
        {
        }

        protected override void Handle(object @event)
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
                        connection.Execute(
                            @"
                            UPDATE [Cart] 
                            SET [Quantity] = [Quantity] + @Quantity
                            WHERE [ItemId] = @ItemId",
                            new { addItemInCartEvent.ItemId, addItemInCartEvent.Quantity }
                        );
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
                    }
                    else
                    {
                        connection.Execute(
                            @"
                            UPDATE [Cart] 
                            SET [Quantity] = [Quantity] - @Quantity
                            WHERE [ItemId] = @ItemId",
                            new { removeItemFromCartEvent.ItemId, removeItemFromCartEvent.Quantity }
                        );
                    }
                }
            }
            if (@event is ResetCartEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    connection.Execute("DELETE FROM [Cart]");
                }
            }
        }
    }
}
