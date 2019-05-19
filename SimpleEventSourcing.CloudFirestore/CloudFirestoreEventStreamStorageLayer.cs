using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SimpleEventSourcing.CloudFirestore.CloudFirestoreConstants;

namespace SimpleEventSourcing.CloudFirestore
{
    public class CloudFirestoreEventStreamStorageLayer<TEvent> : IEventStreamStorageLayer<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly DocumentReference _streamDocumentReference;
        private readonly CollectionReference _eventsCollectionReference;
        private readonly ICloudFirestoreEventConverter<TEvent> _firestoreEventConverter;
        private readonly ICloudFirestoreEventStreamConverter _firestoreEventStreamConverter;

        public CloudFirestoreEventStreamStorageLayer(
            DocumentReference streamDocumentReference,
            ICloudFirestoreEventConverter<TEvent> firestoreEventConverter,
            ICloudFirestoreEventStreamConverter firestoreEventStreamConverter
        )
        {
            _streamDocumentReference = streamDocumentReference;
            _eventsCollectionReference = streamDocumentReference.Collection(EventsCollectionName);
            _firestoreEventConverter = firestoreEventConverter;
            _firestoreEventStreamConverter = firestoreEventStreamConverter;
        }

        private DocumentReference GetEventDocumentReference(TEvent @event)
        {
            return _eventsCollectionReference.Document(@event.Position.ToString());
        }

        public async Task AppendEventAsync(TEvent @event)
        {
            await AppendEventsAsync(new List<TEvent> { @event });
        }

        public async Task AppendEventsAsync(IEnumerable<TEvent> events)
        {
            await _streamDocumentReference.Database.RunTransactionAsync(transaction =>
            {
                foreach (var @event in events)
                {
                    transaction.Create(GetEventDocumentReference(@event), _firestoreEventConverter.ToFirestore(@event));
                }

                transaction.Update(
                    _streamDocumentReference,
                    new Dictionary<string, object>
                    {
                        { "lastPosition", events.Max(e => e.Position) },
                        { "updatedAt", DateTime.Now.ToUniversalTime() }
                    },
                    Precondition.None
                );

                return Task.CompletedTask;
            });
        }

        public async Task<IEnumerable<TEvent>> GetAllEventsAsync()
        {
            var querySnapshot = await _eventsCollectionReference.GetSnapshotAsync();

            return querySnapshot.Documents
                .Select(_firestoreEventConverter.FromFirestore)
                .ToList();
        }

        public async Task<long?> GetCurrentPositionAsync()
        {
            var streamDocument = await _streamDocumentReference.GetSnapshotAsync();
            if (streamDocument.Exists)
            {
                var details = _firestoreEventStreamConverter.FromFirestore(streamDocument);
                return details?.LastPosition;
            }

            return null;
        }

        public async Task<TEvent> GetEventAsync(string eventId)
        {
            var querySnapshot = await _eventsCollectionReference
                .WhereEqualTo("id", eventId)
                .GetSnapshotAsync();

            return querySnapshot.Documents
                .Select(_firestoreEventConverter.FromFirestore)
                .SingleOrDefault();
        }

        public async Task<TEvent> GetEventAsync(int position)
        {
            var snapshot = await _eventsCollectionReference
                .Document(position.ToString())
                .GetSnapshotAsync();

            return _firestoreEventConverter.FromFirestore(snapshot);
        }
    }
}
