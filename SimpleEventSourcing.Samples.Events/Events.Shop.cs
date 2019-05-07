namespace SimpleEventSourcing.Samples.Events
{
    public class CartItemSelected
    {
        public string ItemId { get; set; }
        public long Quantity { get; set; }
    }

    public class CartItemUnselected
    {
        public string ItemId { get; set; }
        public long Quantity { get; set; }
    }

    public class CartReseted { }

    public class OrderedFromCart { }
}
