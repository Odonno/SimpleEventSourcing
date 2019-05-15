using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SimpleEventSourcing.CloudFirestore.CloudFirestoreConstants;

namespace SimpleEventSourcing.CloudFirestore
{
    public class CloudFirestoreEventStreamProvider<TEvent> : IEventStreamProvider<TEvent>
        where TEvent : StreamedEvent, new()
    {
        public IEventStreamProviderStorageLayer<TEvent> StorageProvider { get; }
        public IEventStreamProviderMessagingLayer<TEvent> MessagingProvider { get; }

        public CloudFirestoreEventStreamProvider(
            FirestoreDb firestoreDb,
            ICloudFirestoreEventConverter<TEvent> firestoreEventConverter
        )
        {
            var streamsCollectionReference = firestoreDb.Collection(StreamsCollectionName);

            EventStream<TEvent> createNewStreamFunc(string streamId)
            {
                var streamDocumentReference = streamsCollectionReference.Document(streamId);

                return new EventStream<TEvent>(
                    streamId,
                    new CloudFirestoreEventStreamStorageLayer<TEvent>(streamDocumentReference, firestoreEventConverter),
                    new CloudFirestoreEventStreamMessagingLayer<TEvent>(streamDocumentReference, firestoreEventConverter)
                );
            }

            StorageProvider = new CloudFirestoreEventStreamProviderStorageLayer<TEvent>(createNewStreamFunc, firestoreDb);
            MessagingProvider = new CloudFirestoreEventStreamProviderMessagingLayer<TEvent>(StorageProvider, streamsCollectionReference);
        }

        public Task<IEnumerable<EventStream<TEvent>>> GetAllStreamsAsync()
            => StorageProvider.GetAllStreamsAsync();
        public Task<EventStream<TEvent>> GetStreamAsync(string streamId)
            => StorageProvider.GetStreamAsync(streamId);
        public IObservable<EventStream<TEvent>> ListenForNewStreams()
            => MessagingProvider.ListenForNewStreams();
    }
}