using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Samples.Web
{    
    public class Cart
    {
        public IEnumerable<ItemAndQuantity> Items { get; set; }
    }

    public class Item
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int RemainingQuantity { get; set; }
    }

    public class Order
    {
        public long Id { get; set; }
        public long Number { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsCanceled { get; set; }
        public IEnumerable<ItemAndPriceAndQuantity> Items { get; set; }
    }

    public class ItemAndQuantity
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }
    public class ItemAndPriceAndQuantity
    {
        public long ItemId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
