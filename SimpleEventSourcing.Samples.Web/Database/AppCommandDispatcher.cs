using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Samples.Web.Database
{
    public class AppCommandDispatcher : CommandDispatcher<object, AppEvent>
    {
        protected override IEnumerable<AppEvent> Convert(object command)
        {
            // Either 1 command = a list of events (with correlation id)
            if (command is CreateOrderFromCartCommand)
            {
                var metadata = new { CreatedDate = DateTime.Now, CorrelationId = Guid.NewGuid() };

                return new List<AppEvent>
                {
                    new AppEvent
                    {
                        EventName = GetEventNameFromCommandName(command.GetType().Name),
                        Data = command,
                        Metadata = metadata
                    },
                    new AppEvent
                    {
                        EventName = GetEventNameFromCommandName(typeof(ResetCartCommand).Name),
                        Data = new ResetCartCommand(),
                        Metadata = metadata
                    }
                };
            }

            // Or a 1 command = 1 event pattern
            return new List<AppEvent>
            {
                new AppEvent
                {
                    EventName = GetEventNameFromCommandName(command.GetType().Name),
                    Data = command,
                    Metadata = new { CreatedDate = DateTime.Now }
                }
            };
        }

        private string GetEventNameFromCommandName(string commandName)
        {
            switch (commandName)
            {
                case nameof(AddItemInCartCommand):
                    return "CartItemSelected";
                case nameof(RemoveItemFromCartCommand):
                    return "CartItemUnselected";
                case nameof(ResetCartCommand):
                    return "CartReseted";
                case nameof(CreateOrderFromCartCommand):
                    return "OrderedFromCart";
                case nameof(ValidateOrderCommand):
                    return "OrderValidated";
                case nameof(CancelOrderCommand):
                    return "OrderCanceled";
                case nameof(CreateItemCommand):
                    return "ItemRegistered";
                case nameof(UpdateItemPriceCommand):
                    return "ItemPriceUpdated";
                case nameof(SupplyItemCommand):
                    return "ItemSupplied";
            }
            throw new NotImplementedException();
        }
    }
}
