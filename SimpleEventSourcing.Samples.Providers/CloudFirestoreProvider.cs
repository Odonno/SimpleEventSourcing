using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Grpc.Core;
using System;

namespace SimpleEventSourcing.Samples.Providers
{
    public class CloudFirestoreProvider
    {
        public FirestoreDb Database { get; }

        /// <summary>
        /// Create a new firestore client instance
        /// </summary>
        /// <param name="projectName">Name of the project</param>
        /// <param name="jsonCredentialPath">Path to the json credentials file</param>
        public CloudFirestoreProvider(string projectName, string jsonCredentialPath)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", AppDomain.CurrentDomain.BaseDirectory + jsonCredentialPath);

            var credentials = GoogleCredential.GetApplicationDefault();
            var channelCredentials = credentials.ToChannelCredentials();
            var channel = new Channel(FirestoreClient.DefaultEndpoint.ToString(), channelCredentials);
            var firestoreClient = FirestoreClient.Create(channel);
            Database = FirestoreDb.Create(projectName, firestoreClient);
        }
    }
}
