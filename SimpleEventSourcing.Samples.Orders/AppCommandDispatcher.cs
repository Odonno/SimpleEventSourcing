using SimpleEventSourcing.Samples.EventStore;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Samples.Orders
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
                    Data = command,
                    Metadata = new SimpleEventMetadata { CreatedDate = DateTime.Now }
                }
            };
        }

        private string GetEventNameFromCommandName(string commandName)
        {
            switch (commandName)
            {
                case nameof(ValidateOrderCommand):
                    return "OrderValidated";
                case nameof(CancelOrderCommand):
                    return "OrderCanceled";
            }
            throw new NotImplementedException();
        }
    }
}
