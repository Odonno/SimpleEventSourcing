using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static SimpleEventSourcing.CloudFirestore.CloudFirestoreConstants;

namespace SimpleEventSourcing.CloudFirestore
{
    public sealed class CloudFirestoreEventStream<TEvent> : IRealtimeEventStream<TEvent>
        where TEvent : StreamedEvent, new()
    {
        private readonly DocumentReference _streamDocumentReference;
        private readonly CollectionReference _eventsCollectionReference;
        private readonly ICloudFirestoreEventConverter<TEvent> _firestoreEventConverter;

        public string Id { get; }

        public CloudFirestoreEventStream(
            string streamId,
            DocumentReference streamDocumentReference,
            ICloudFirestoreEventConverter<TEvent> firestoreEventConverter
        )
        {
            Id = streamId;

            _streamDocumentReference = streamDocumentReference;
            _eventsCollectionReference = streamDocumentReference.Collection(EventsCollectionName);

            _firestoreEventConverter = firestoreEventConverter;
        }

        private DocumentReference GetEventDocumentReference(TEvent @event)
        {
            return _eventsCollectionReference.Document(@event.Position.ToString());
        }

        public async Task<long?> GetCurrentPositionAsync()
        {
            var stream = await _streamDocumentReference.GetSnapshotAsync();
            if (stream.Exists)
            {
                var details = stream.ConvertTo<EventStreamDetails>();
                return details?.LastPosition;
            }

            return null;
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
                .Select(snapshot => snapshot.ConvertTo<TEvent>())
                .ToList();
        }

        public async Task<TEvent> GetEventAsync(string eventId)
        {
            var querySnapshot = await _eventsCollectionReference
                .WhereEqualTo("id", eventId)
                .GetSnapshotAsync();

            return querySnapshot.Documents
                .Select(snapshot => snapshot.ConvertTo<TEvent>())
                .SingleOrDefault();
        }

        public async Task<TEvent> GetEventAsync(int position)
        {
            var snapshot = await _eventsCollectionReference
                .Document(position.ToString())
                .GetSnapshotAsync();

            return snapshot.ConvertTo<TEvent>();
        }

        private static bool ShouldListenForNewEvents(bool isNewStream, DateTime startListeningAt, Timestamp? documentCreatedAt)
        {
            return isNewStream || 
                (documentCreatedAt.HasValue && documentCreatedAt.Value.ToDateTime() > startListeningAt);
        }
        public IObservable<TEvent> ListenForNewEvents(bool isNewStream)
        {
            return Observable.Create<TEvent>(observer =>
            {
                var startListeningAt = DateTime.Now;

                var listener = _eventsCollectionReference.Listen(querySnapshot =>
                {
                    foreach (var change in querySnapshot.Changes)
                    {
                        var document = change.Document;
                        if (document.Exists)
                        {
                            if (ShouldListenForNewEvents(isNewStream, startListeningAt, document.CreateTime))
                            {
                                observer.OnNext(_firestoreEventConverter.FromFirestore(document));
                            }
                        }
                    }
                });

                return Disposable.Create(async () =>
                {
                    await listener.StopAsync();
                });
            });
        }
    }
}
