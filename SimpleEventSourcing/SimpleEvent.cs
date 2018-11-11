namespace SimpleEventSourcing
{
    /// <summary>
    /// A simple event implementation with the minimum of information.
    /// </summary>
    public class SimpleEvent
    {
        /// <summary>
        /// Name of the event.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// The container of the data representing the event.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// The metadate of the event that add non-business details, like the date of a creation or a correlation id.
        /// </summary>
        public object Metadata { get; set; }
    }
}
