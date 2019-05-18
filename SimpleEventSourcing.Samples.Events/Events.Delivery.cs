using System.Collections.Generic;

namespace SimpleEventSourcing.Samples.Events
{
    public class OrderCreated
    {
        public class OrderedItem
        {
            public string ItemId { get; set; }
            public long Quantity { get; set; }
        }

        public string Id { get; set; }
        public List<OrderedItem> Items { get; set; }
    }

    public class OrderValidated
    {
        public string OrderId { get; set; }
    }

    public class OrderCanceled
    {
        public string OrderId { get; set; }
    }
}
