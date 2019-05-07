namespace SimpleEventSourcing.CloudFirestore
{
    public static class CloudFirestoreConstants
    {
        /// <summary>
        /// The name of the Cloud Firestore collection used to store all streams.
        /// </summary>
        public const string StreamsCollectionName = "streams";

        /// <summary>
        /// The name of the Cloud Firestore collection used to store all events of a dedicated stream.
        /// </summary>
        public const string EventsCollectionName = "events";
    }
}
