using System;

namespace SimpleEventSourcing.Samples
{
    public class EventInfo
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Data { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CartItem
    {
        public string Name { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class Cart
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int NumberOfUnits { get; set; }
    }
}
