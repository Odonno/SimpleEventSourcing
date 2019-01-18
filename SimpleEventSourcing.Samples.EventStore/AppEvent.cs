namespace SimpleEventSourcing.Samples.EventStore
{
    public class AppEvent : SimpleEvent
    {
        public string Id { get; set; }
    }

    public class EventDbo
    {
        public string Id { get; set; }
        public string EventName { get; set; }
        public string Data { get; set; }
        public string Metadata { get; set; }
    }
}
