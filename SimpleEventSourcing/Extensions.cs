using System.Collections.Generic;
using static System.String;
using static System.Guid;
using System.Linq;

namespace SimpleEventSourcing
{
    public static class Extensions
    {
        /// <summary>
        /// Check if the string provided is a valid guid.
        /// </summary>
        /// <param name="value">The string containing the guid.</param>
        /// <returns>True if the value is a valid guid.</returns>
        public static bool IsValidGuid(string value)
        {
            return !IsNullOrWhiteSpace(value) && TryParse(value, out var _);
        }

        /// <summary>
        /// Ensure that all events have an id, if not a new one will be generated on the fly.
        /// </summary>
        /// <param name="events">A list of events.</param>
        public static void EnsureEachEventHasId<TEvent>(IEnumerable<TEvent> events)
            where TEvent : StreamedEvent
        {
            foreach (var @event in events)
            {
                if (!IsValidGuid(@event.Id))
                {
                    @event.Id = NewGuid().ToString();
                }
            }
        }

        /// <summary>
        /// Create an enumerable of items based on a list of parameters.
        /// </summary>
        /// <typeparam name="T">Type of item.</typeparam>
        /// <param name="array">List of items in parameters.</param>
        /// <returns>Returns an enumerable of items.</returns>
        public static IEnumerable<T> List<T>(params T[] array)
        {
            return array;
        }

        /// <summary>
        /// Create a list of streamed events
        /// </summary>
        /// <param name="streamId">Id of the stream.</param>
        /// <param name="streamCurrentPosition">Current position of the stream when creating those events.</param>
        /// <param name="dataEvents">List of events data to be stored.</param>
        /// <returns>Returns a list of <see cref="StreamedEvent"/>.</returns>
        public static IEnumerable<StreamedEvent> CreateStreamedEvents(string streamId, long? streamCurrentPosition, IEnumerable<object> dataEvents)
        {
            bool hasCorrelation = dataEvents.Count() > 1;
            string correlationId = hasCorrelation ? NewGuid().ToString() : null;

            return dataEvents
                .Select((dataEvent, index) =>
                {
                    return new StreamedEvent
                    {
                        StreamId = streamId,
                        Position = streamCurrentPosition.HasValue
                            ? streamCurrentPosition.Value + index + 1
                            : 1,
                        Id = NewGuid().ToString(),
                        EventName = dataEvent.GetType().Name,
                        Data = dataEvent,
                        Metadata = StreamedEventMetadata.WithCorrelation(correlationId) // TODO : static extensions
                    };
                });
        }
    }
}
