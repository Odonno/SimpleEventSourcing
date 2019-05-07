namespace SimpleEventSourcing.UnitTests.Models
{
    public class AddItemInCartCommand
    {
        public string ItemName { get; set; }
        public decimal UnitCost { get; set; }
        public int NumberOfUnits { get; set; } = 1;
    }

    public class RemoveItemFromCartCommand
    {
        public string ItemName { get; set; }
        public int NumberOfUnits { get; set; } = 1;
    }

    public class ResetCartCommand { }
}
