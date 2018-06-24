using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using static SimpleEventSourcing.Samples.DatabaseConfiguration;

namespace SimpleEventSourcing.Samples.Database.Version2
{
    public class CartTableEventView : EventView
    {
        private readonly IEnumerable<CartItem> _cartItems;

        public CartTableEventView(IObservable<object> events) : base(events)
        {
            using (var connection = GetViewsDatabaseConnection())
            {
                _cartItems = connection.Query<CartItem>("SELECT * FROM [Item]");
            }
        }

        protected override void Handle(object @event)
        {
            if (@event is AddItemInCartEvent addItemInCartEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    bool canUpdate = connection
                        .Query<int>("SELECT COUNT(*) FROM [Cart] WHERE [ItemName] = @ItemName", new { addItemInCartEvent.ItemName })
                        .Single() > 0;

                    if (canUpdate)
                    {
                        connection.Execute(
                            @"
                            UPDATE [Cart] 
                            SET [NumberOfUnits] = [NumberOfUnits] + @NumberOfUnitsToAdd, [UpdatedDate] = @CurrentDate
                            WHERE [ItemName] = @ItemName",
                            new { addItemInCartEvent.ItemName, NumberOfUnitsToAdd = addItemInCartEvent.NumberOfUnits, CurrentDate = DateTime.Now }
                        );
                    }
                    else
                    {
                        connection.Execute(
                            @"
                            INSERT INTO [Cart] 
                            ([ItemName], [NumberOfUnits], [CreatedDate])
                            VALUES (@ItemName, @NumberOfUnitsToAdd, @CurrentDate)",
                            new { addItemInCartEvent.ItemName, NumberOfUnitsToAdd = addItemInCartEvent.NumberOfUnits, CurrentDate = DateTime.Now }
                        );
                    }
                }
            }
            if (@event is RemoveItemFromCartEvent removeItemFromCartEvent)
            {
                using (var connection = GetViewsDatabaseConnection())
                {
                    connection.Execute(
                        @"
                        UPDATE [Cart] 
                        SET [NumberOfUnits] = [NumberOfUnits] - 1, [UpdatedDate] = @CurrentDate
                        WHERE [ItemName] = @ItemName",
                        new { removeItemFromCartEvent.ItemName, CurrentDate = DateTime.Now }
                    );
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
