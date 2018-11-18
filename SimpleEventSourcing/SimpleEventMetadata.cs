using System;

namespace SimpleEventSourcing
{
    /// <summary>
    /// A simple event metadata implementation with the minimum of information.
    /// </summary>
    public class SimpleEventMetadata
    {
        /// <summary>
        /// The date of creation of an event.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The correlation id between multiple events.
        /// </summary>
        public string CorrelationId { get; set; }
    }
}
