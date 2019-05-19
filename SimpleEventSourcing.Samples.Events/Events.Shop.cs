using System.Collections.Generic;

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

    public class OrderedFromCart
    {
        public class OrderedItem
        {
            public string ItemId { get; set; }
            public long Quantity { get; set; }
        }

        public List<OrderedItem> Items { get; set; }
    }
}
