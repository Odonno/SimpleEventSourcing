using Converto;
using SimpleEventSourcing.Samples.EventStore;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Samples.Inventory
{
    public class AppCommandDispatcher : CommandDispatcher<object, AppEvent>
    {
        protected override IEnumerable<AppEvent> Convert(object command)
        {
            // Or a 1 command = 1 event pattern
            return new List<AppEvent>
            {
                new AppEvent
                {
                    EventName = GetEventNameFromCommandName(command.GetType().Name),
                    Data = EnsureIdExists(command),
                    Metadata = new SimpleEventMetadata { CreatedDate = DateTime.Now }
                }
            };
        }

        private string GetEventNameFromCommandName(string commandName)
        {
            switch (commandName)
            {
                case nameof(CreateItemCommand):
                    return "ItemRegistered";
                case nameof(UpdateItemPriceCommand):
                    return "ItemPriceUpdated";
                case nameof(SupplyItemCommand):
                    return "ItemSupplied";
            }
            throw new NotImplementedException();
        }

        private object EnsureIdExists(object command)
        {
            if (command is CreateItemCommand createItemCommand && !IsValidGuid(createItemCommand.Id))
            {
                return command.With(new { Id = Guid.NewGuid().ToString() });
            }
            return command;
        }

        private bool IsValidGuid(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out var _);
        }
    }
}
