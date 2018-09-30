using System;

namespace SimpleEventSourcing.Samples.Web
{
    public class AddItemInCartEvent
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveItemFromCartEvent
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class ResetCartEvent { }

    public class CreateOrderFromCartEvent
    {
        public DateTime Date { get; set; }
    }

    public class ValidateOrderEvent
    {
        public long OrderId { get; set; }
    }

    public class CancelOrderEvent
    {
        public long OrderId { get; set; }
    }

    public class CreateItemEvent
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int InitialQuantity { get; set; }
    }

    public class UpdateItemPriceEvent
    {
        public long ItemId { get; set; }
        public decimal NewPrice { get; set; }
    }

    public class SupplyItemEvent
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
