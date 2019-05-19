using Google.Cloud.Firestore;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SimpleEventSourcing.CloudFirestore
{
    public class CloudFirestoreEventStreamProviderMessagingLayer<TEvent> : IEventStreamProviderMessagingLayer<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly IEventStreamProviderStorageLayer<TEvent> _eventStreamProviderStorageLayer;
        private readonly Subject<EventStream<TEvent>> _newStreamsSubject = new Subject<EventStream<TEvent>>();

        public CloudFirestoreEventStreamProviderMessagingLayer(
            IEventStreamProviderStorageLayer<TEvent> eventStreamProviderStorageLayer,
            CollectionReference streamsCollectionReference
        )
        {
            _eventStreamProviderStorageLayer = eventStreamProviderStorageLayer;

            var newStreams = new ConcurrentBag<string>();

            ListenNewStreams(streamsCollectionReference)
                .Where(documentSnapshot => !newStreams.Contains(documentSnapshot.Id))
                .Subscribe(async documentSnapshot =>
                {
                    string streamId = documentSnapshot.Id;

                    newStreams.Add(streamId);
                    var stream = await _eventStreamProviderStorageLayer.GetStreamAsync(streamId);

                    _newStreamsSubject.OnNext(stream);
                });
        }

        private static bool ShouldListenForNewStreams(DateTime startListeningAt, Timestamp? documentCreatedAt)
        {
            return documentCreatedAt.HasValue && documentCreatedAt.Value.ToDateTime() > startListeningAt.ToUniversalTime();
        }
        private static IObservable<DocumentSnapshot> ListenNewStreams(CollectionReference streamsCollectionReference)
        {
            return Observable.Create<DocumentSnapshot>(observer =>
            {
                var startListeningAt = DateTime.Now;

                var listener = streamsCollectionReference.Listen(querySnapshot =>
                {
                    foreach (var change in querySnapshot.Changes)
                    {
                        var document = change.Document;
                        if (document.Exists)
                        {
                            if (ShouldListenForNewStreams(startListeningAt, document.CreateTime))
                            {
                                observer.OnNext(document);
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

        public IObservable<EventStream<TEvent>> ListenForNewStreams()
        {
            return _newStreamsSubject;
        }
    }
}
