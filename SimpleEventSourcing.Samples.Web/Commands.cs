using System;

namespace SimpleEventSourcing.Samples.Web
{
    public class AddItemInCartCommand
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveItemFromCartCommand
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class ResetCartCommand { }

    public class CreateOrderFromCartCommand { }

    public class ValidateOrderCommand
    {
        public long OrderId { get; set; }
    }

    public class CancelOrderCommand
    {
        public long OrderId { get; set; }
    }

    public class CreateItemCommand
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int InitialQuantity { get; set; }
    }

    public class UpdateItemPriceCommand
    {
        public long ItemId { get; set; }
        public decimal NewPrice { get; set; }
    }

    public class SupplyItemCommand
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
