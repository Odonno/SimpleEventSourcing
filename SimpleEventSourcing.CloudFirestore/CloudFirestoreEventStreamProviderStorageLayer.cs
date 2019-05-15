using Google.Cloud.Firestore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SimpleEventSourcing.CloudFirestore.CloudFirestoreConstants;

namespace SimpleEventSourcing.CloudFirestore
{
    public class CloudFirestoreEventStreamProviderStorageLayer<TEvent> : IEventStreamProviderStorageLayer<TEvent>
        where TEvent : StreamedEvent
    {
        private readonly CollectionReference _streamsCollectionReference;
        private readonly ConcurrentDictionary<string, EventStream<TEvent>> _streams = new ConcurrentDictionary<string, EventStream<TEvent>>();
        private readonly Func<string, EventStream<TEvent>> _createNewStreamFunc;

        public CloudFirestoreEventStreamProviderStorageLayer(
            Func<string, EventStream<TEvent>> createNewStreamFunc,
            FirestoreDb firestoreDb
        )
        {
            _streamsCollectionReference = firestoreDb.Collection(StreamsCollectionName);
            _createNewStreamFunc = createNewStreamFunc;
        }

        public async Task<IEnumerable<EventStream<TEvent>>> GetAllStreamsAsync()
        {
            var streams = new List<EventStream<TEvent>>();
            var enumerator = _streamsCollectionReference.ListDocumentsAsync().GetEnumerator();

            while (await enumerator.MoveNext())
            {
                var stream = await GetStreamAsync(enumerator.Current.Id);
                streams.Add(stream);
            }

            return streams;
        }

        public Task<EventStream<TEvent>> GetStreamAsync(string streamId)
        {
            var stream = _streams.GetOrAdd(
                streamId,
                _createNewStreamFunc
            );

            return Task.FromResult(stream);
        }
    }
}
