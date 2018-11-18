using System;

namespace SimpleEventSourcing.Samples.Web
{
    public class EventDbo
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Data { get; set; }
        public string Metadata { get; set; }
    }

    public class OrderDbo
    {
        public long Id { get; set; }
        public long Number { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsCanceled { get; set; }
    }

    public class ItemOrderedDbo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ItemId { get; set; }
        public int Quantity { get; set; }
        public string Price { get; set; }
    }

    public class ItemDbo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public int RemainingQuantity { get; set; }
    }
}
