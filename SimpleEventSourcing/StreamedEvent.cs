namespace SimpleEventSourcing
{
    /// <summary>
    /// A simple event implementation with the minimum of information.
    /// An event is stored in a database inside a specific stream.
    /// </summary>
    public class StreamedEvent
    {
        /// <summary>
        /// Id of the stream where this event is stored.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Position of the event in the stream
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// Id of the event.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the event.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// The container of the data representing the event.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// The metadate of the event that add non-business details, like the date of creation or a correlation id.
        /// </summary>
        public StreamedEventMetadata Metadata { get; set; }
    }
}
