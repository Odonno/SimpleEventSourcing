using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using static SimpleEventSourcing.CosmosDb.CosmosDbConstants;
using static Microsoft.Azure.Documents.Client.UriFactory;

namespace SimpleEventSourcing.CosmosDb
{
    internal static class CosmosDbExtensions
    {
        internal static async Task CreateDatabaseAsync(this DocumentClient client, string databaseName)
        {
            var database = new Database { Id = databaseName };
            await client.CreateDatabaseIfNotExistsAsync(database);
        }

        internal static async Task CreateCollectionAsync(this DocumentClient client, string databaseName, int throughput = MinimumThroughput)
        {
            var streamPath = new Collection<string>
            {
                "/" + StreamPartitionKey
            };

            var keys = new Collection<UniqueKey>
            {
                new UniqueKey { Paths = streamPath }
            };

            var collection = new DocumentCollection { Id = CollectionName };
            collection.UniqueKeyPolicy = new UniqueKeyPolicy { UniqueKeys = keys };
            collection.PartitionKey.Paths.Add("/" + StreamPartitionKey);

            var requestOptions = new RequestOptions
            {
                OfferThroughput = CorrectThroughput(throughput)
            };

            var databaseUri = CreateDatabaseUri(databaseName);

            await client.CreateDocumentCollectionIfNotExistsAsync(databaseUri, collection, requestOptions);
        }

        internal static async Task AppendEvent<TEvent>(this DocumentClient client, string databaseName, string streamId, TEvent @event)
        {
            await client.CreateDocumentAsync(
                CreateDocumentCollectionUri(databaseName, CollectionName),
                @event
            );
        }

        private static int CorrectThroughput(int i)
        {
            int throughput = (int)(Math.Round(i / 100m, 0) * 100m);

            if (throughput > MaximumThroughput) return MaximumThroughput;
            if (throughput < MinimumThroughput) return MinimumThroughput;
            return throughput;
        }
    }
}
