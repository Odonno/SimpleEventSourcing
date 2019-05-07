using Google.Cloud.Firestore;

namespace SimpleEventSourcing.CloudFirestore
{
    /// <summary>
    /// Convert an event that comes from/saved to Cloud Firestore
    /// </summary>
    /// <typeparam name="TEvent">Type of the events stored.</typeparam>
    public interface ICloudFirestoreEventConverter<TEvent>
        where TEvent : StreamedEvent
    {
        TEvent FromFirestore(DocumentSnapshot documentSnapshot);
        object ToFirestore(TEvent @event);
    }
}
