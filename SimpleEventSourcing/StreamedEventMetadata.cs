using System;

namespace SimpleEventSourcing
{
    /// <summary>
    /// A simple event metadata implementation with the minimum of information.
    /// </summary>
    public class StreamedEventMetadata
    {
        /// <summary>
        /// The date of creation of an event.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The correlation id between multiple events.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// A default event metadata (with the CreatedDate field)
        /// </summary>
        public static StreamedEventMetadata Default
            => new StreamedEventMetadata { CreatedAt = DateTime.Now.ToUniversalTime() };

        /// <summary>
        /// A default event metadata (with both CreatedDate and CorrelationId fields)
        /// </summary>
        public static StreamedEventMetadata WithCorrelation(string correlationId) =>
            new StreamedEventMetadata { CreatedAt = DateTime.Now.ToUniversalTime(), CorrelationId = correlationId };
    }
}
