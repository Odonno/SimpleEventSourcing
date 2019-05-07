using System;

namespace SimpleEventSourcing
{
    /// <summary>
    /// Information about an Event Stream.
    /// </summary>
    public class EventStreamDetails
    {
        public long LastPosition { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
