using Google.Cloud.Firestore;

namespace SimpleEventSourcing.CloudFirestore
{
    /// <summary>
    /// Convert a event stream data that comes from/saved to Cloud Firestore.
    /// </summary>
    public interface ICloudFirestoreEventStreamConverter
    {
        EventStreamDetails FromFirestore(DocumentSnapshot documentSnapshot);
        object ToFirestore(EventStreamDetails @event);
    }
}
