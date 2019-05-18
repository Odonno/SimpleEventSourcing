using Google.Cloud.Firestore;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using static SimpleEventSourcing.CloudFirestore.CloudFirestoreConstants;

namespace SimpleEventSourcing.CloudFirestore
{
    public class CloudFirestoreEventStreamMessagingLayer<TEvent> : IEventStreamMessagingLayer<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly CollectionReference _eventsCollectionReference;
        private readonly ICloudFirestoreEventConverter<TEvent> _firestoreEventConverter;

        public CloudFirestoreEventStreamMessagingLayer(
            DocumentReference streamDocumentReference,
            ICloudFirestoreEventConverter<TEvent> firestoreEventConverter
        )
        {
            _eventsCollectionReference = streamDocumentReference.Collection(EventsCollectionName);
            _firestoreEventConverter = firestoreEventConverter;
        }

        private static bool ShouldListenForNewEvents(bool isNewStream, DateTime startListeningAt, Timestamp? documentCreatedAt)
        {
            return isNewStream ||
                (documentCreatedAt.HasValue && documentCreatedAt.Value.ToDateTime() > startListeningAt.ToUniversalTime());
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
