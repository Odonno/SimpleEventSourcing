namespace SimpleEventSourcing.CosmosDb
{
    public static class CosmosDbConstants
    {
        /// <summary>
        /// The name of the collection used to store events.
        /// </summary>
        public const string CollectionName = "Events";

        /// <summary>
        /// The name of the partition key (meaning the stream id to aggregate events).
        /// </summary>
        public const string StreamPartitionKey = "StreamId";

        /// <summary>
        /// Minimum throughput in a CosmosDb collection.
        /// </summary>
        public const int MinimumThroughput = 400;

        /// <summary>
        /// Maximum throughput in a CosmosDb collection.
        /// </summary>
        public const int MaximumThroughput = 1000 * 1000;
    }
}
