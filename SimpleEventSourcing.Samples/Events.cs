namespace SimpleEventSourcing.Samples
{
    public class AddItemInCartEvent
    {
        public string ItemName { get; set; }
        public int NumberOfUnits { get; set; } = 1;
    }

    public class RemoveItemFromCartEvent
    {
        public string ItemName { get; set; }
    }

    public class ResetCartEvent
    {
    }
}
