namespace SimpleEventSourcing.Samples.EventStore
{
    public class CartItemSelected
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItemUnselected
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartReseted { }

    public class OrderedFromCart { }

    public class OrderValidated
    {
        public string OrderId { get; set; }
    }

    public class OrderCanceled
    {
        public string OrderId { get; set; }
    }

    public class ItemRegistered
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int InitialQuantity { get; set; }
    }

    public class ItemPriceUpdated
    {
        public string ItemId { get; set; }
        public decimal NewPrice { get; set; }
    }

    public class ItemSupplied
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
