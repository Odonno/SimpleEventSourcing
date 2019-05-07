using Google.Cloud.Firestore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using static SimpleEventSourcing.CloudFirestore.CloudFirestoreConstants;

namespace SimpleEventSourcing.CloudFirestore
{
    public sealed class CloudFirestoreEventStreamProvider<TEvent> : IRealtimeEventStreamProvider<TEvent>
        where TEvent : StreamedEvent, new()
    {
        private readonly CollectionReference _streamsCollectionReference;
        private readonly ConcurrentDictionary<string, IEventStream<TEvent>> _streams = new ConcurrentDictionary<string, IEventStream<TEvent>>();
        private readonly Subject<IEventStream<TEvent>> _newStreamsSubject = new Subject<IEventStream<TEvent>>();
        private readonly ICloudFirestoreEventConverter<TEvent> _firestoreEventConverter;

        public CloudFirestoreEventStreamProvider(FirestoreDb firestoreDb, ICloudFirestoreEventConverter<TEvent> firestoreEventConverter)
        {
            _streamsCollectionReference = firestoreDb.Collection(StreamsCollectionName);
            _firestoreEventConverter = firestoreEventConverter;

            ListenNewStreams().Subscribe(documentSnapshot =>
            {
                string streamId = documentSnapshot.Id;
                var streamDocumentReference = _streamsCollectionReference.Document(streamId);
                var stream = new CloudFirestoreEventStream<TEvent>(streamId, streamDocumentReference, firestoreEventConverter);

                _streams.TryAdd(streamId, stream);
                _newStreamsSubject.OnNext(stream);
            });
        }

        private static bool ShouldListenForNewStreams(DateTime startListeningAt, Timestamp? documentCreatedAt)
        {
            return documentCreatedAt.HasValue && documentCreatedAt.Value.ToDateTime() > startListeningAt;
        }
        private IObservable<DocumentSnapshot> ListenNewStreams()
        {
            return Observable.Create<DocumentSnapshot>(observer =>
            {
                var startListeningAt = DateTime.Now;

                var listener = _streamsCollectionReference.Listen(querySnapshot =>
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

        public IObservable<IEventStream<TEvent>> DetectNewStreams()
        {
            return _newStreamsSubject;
        }

        public async Task<IEnumerable<IEventStream<TEvent>>> GetAllStreamsAsync()
        {
            var streams = new List<IEventStream<TEvent>>();
            var enumerator = _streamsCollectionReference.ListDocumentsAsync().GetEnumerator();

            while (await enumerator.MoveNext())
            {
                var stream = await GetStreamAsync(enumerator.Current.Id);
                streams.Add(stream);
            }

            return streams;
        }

        public Task<IEventStream<TEvent>> GetStreamAsync(string streamId)
        {
            var stream = _streams.GetOrAdd(
                streamId,
                CreateNewStream
            );

            return Task.FromResult(stream);
        }

        private Func<string, IEventStream<TEvent>> CreateNewStream =>
            (string streamId) =>
                new CloudFirestoreEventStream<TEvent>(
                    streamId, 
                    _streamsCollectionReference.Document(streamId), 
                    _firestoreEventConverter
                );
    }
}
