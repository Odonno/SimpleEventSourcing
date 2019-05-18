using Google.Cloud.Firestore;
using SimpleEventSourcing.CloudFirestore;
using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.Samples.Providers
{
    public class EventStreamFirestoreConverter : ICloudFirestoreEventStreamConverter
    {
        public EventStreamDetails FromFirestore(DocumentSnapshot documentSnapshot)
        {
            var dataDictionary = documentSnapshot.ToDictionary();

            return new EventStreamDetails
            {
                LastPosition = (long)dataDictionary["lastPosition"],
                UpdatedAt = ((Timestamp)dataDictionary["updatedAt"]).ToDateTime()
            };
        }

        public object ToFirestore(EventStreamDetails eventStreamDetails)
        {
            return new Dictionary<string, object>
            {
                { "lastPosition", eventStreamDetails.LastPosition },
                { "updatedAt", eventStreamDetails.UpdatedAt.ToUniversalTime() }
            };
        }
    }
}
