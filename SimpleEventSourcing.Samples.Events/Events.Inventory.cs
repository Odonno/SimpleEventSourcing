namespace SimpleEventSourcing.Samples.Events
{
    public class ItemRegistered
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public long InitialQuantity { get; set; }
    }

    public class ItemPriceUpdated
    {
        public string ItemId { get; set; }
        public double NewPrice { get; set; }
    }

    public class ItemSupplied
    {
        public string ItemId { get; set; }
        public long Quantity { get; set; }
    }

    public class ItemReserved
    {
        public string ItemId { get; set; }
        public long Quantity { get; set; }
        public string OrderId { get; set; }
    }

    public class ItemShipped
    {
        public string ItemId { get; set; }
        public long Quantity { get; set; }
        public string OrderId { get; set; }
    }
}
