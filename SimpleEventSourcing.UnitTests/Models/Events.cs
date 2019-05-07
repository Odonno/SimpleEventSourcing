namespace SimpleEventSourcing.UnitTests.Models
{
    public class ItemAddedInCart
    {
        public string ItemName { get; set; }
        public decimal UnitCost { get; set; }
        public int NumberOfUnits { get; set; }
    }

    public class ItemRemovedFromCart
    {
        public string ItemName { get; set; }
        public int NumberOfUnits { get; set; }
    }

    public class CartReset { }
}
