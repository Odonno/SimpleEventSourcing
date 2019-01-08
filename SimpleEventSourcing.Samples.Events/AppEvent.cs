namespace SimpleEventSourcing.Samples.Events
{
    public class AppEvent : SimpleEvent
    {
        public string Id { get; set; }
        public int? Number { get; set; }
    }

    public class EventDbo
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public string EventName { get; set; }
        public string Data { get; set; }
        public string Metadata { get; set; }
    }
}
