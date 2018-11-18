namespace SimpleEventSourcing.Samples.Web
{
    public class CartItemSelected
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItemUnselected
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartReseted { }

    public class OrderedFromCart { }

    public class OrderValidated
    {
        public long OrderId { get; set; }
    }

    public class OrderCanceled
    {
        public long OrderId { get; set; }
    }

    public class ItemRegistered
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int InitialQuantity { get; set; }
    }

    public class ItemPriceUpdated
    {
        public long ItemId { get; set; }
        public decimal NewPrice { get; set; }
    }

    public class ItemSupplied
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
