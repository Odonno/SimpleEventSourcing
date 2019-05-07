namespace SimpleEventSourcing.UnitTests.Models
{
    public class TotalCostCartState
    {
        public decimal TotalCost { get; set; }
    }
    public class TotalCostCartEventView : InMemoryEventView<StreamedEvent, TotalCostCartState>
    {
        public TotalCostCartEventView(IEventStreamProvider<StreamedEvent> streamProvider, TotalCostCartState initialState = null) : base(streamProvider, initialState)
        {
        }

        protected override TotalCostCartState Reduce(TotalCostCartState state, StreamedEvent @event)
        {
            if (@event.EventName == nameof(AddItemInCartCommand))
            {
                var data = @event.Data as AddItemInCartCommand;
                return new TotalCostCartState
                {
                    TotalCost = state.TotalCost + (data.NumberOfUnits * data.UnitCost)
                };
            }
            if (@event.EventName == nameof(RemoveItemFromCartCommand))
            {
                var data = @event.Data as RemoveItemFromCartCommand;
                int meanCostPerUnit = 10; // TODO : calculate mean cost per unit

                return new TotalCostCartState
                {
                    TotalCost = state.TotalCost - (meanCostPerUnit * data.NumberOfUnits)
                };
            }
            if (@event.EventName == nameof(ResetCartCommand))
            {
                return new TotalCostCartState
                {
                    TotalCost = 0
                };
            }
            return state;
        }
    }
}
