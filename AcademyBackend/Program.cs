using System;

namespace AcademyBackend
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    class Program
    {
        private static DocumentClient client;

        private static readonly string DatabaseName = "";
        private static readonly string CollectionName = "";
        private static readonly string endpointUrl = "";
        private static readonly string authorizationKey = "";

        public static void Main(string[] args)
        {
            try
            {
                //Get a Document client
                using (client = new DocumentClient(new Uri(endpointUrl), authorizationKey,
                    new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp }))
                {
                    RunChangeFeedAsync(DatabaseName, CollectionName).Wait();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadLine();
        }

        private static async Task RunChangeFeedAsync(string databaseId, string collectionId)
        {
            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });

            DocumentCollection collectionDefinition = new DocumentCollection();
            collectionDefinition.Id = collectionId;
            collectionDefinition.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });
            collectionDefinition.PartitionKey.Paths.Add("/UserId");

            await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseId),
                collectionDefinition,
                new RequestOptions { OfferThroughput = 2500 });

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

            //Console.WriteLine("Inserting 10 documents");
            //List<Task> insertTasks = new List<Task>();

            //for (int i = 0; i < 10; i++)
            //{
            //    insertTasks.Add(client.CreateDocumentAsync(
            //        collectionUri,
            //        new BasicTask() { UserId = Guid.NewGuid().ToString(), Added = DateTime.Now, Description = $"Buy {i} apples", IsComplete = false, DueBy = DateTime.Now.AddDays(i * -1), TaskId = Guid.NewGuid() }));
            //}
            //await Task.WhenAll(insertTasks);

            // Returns all documents in the collection.
            Console.WriteLine("Reading all changes from the beginning");
            Dictionary<string, string> checkpoints = await GetChanges(client, collectionUri, new Dictionary<string, string>());
        }

        private static async Task<Dictionary<string, string>> GetChanges(
            DocumentClient client,
            Uri collectionUri,
            Dictionary<string, string> checkpoints)
        {
            int numChangesRead = 0;
            string pkRangesResponseContinuation = null;
            List<PartitionKeyRange> partitionKeyRanges = new List<PartitionKeyRange>();

            do
            {
                FeedResponse<PartitionKeyRange> pkRangesResponse = await client.ReadPartitionKeyRangeFeedAsync(
                    collectionUri,
                    new FeedOptions { RequestContinuation = pkRangesResponseContinuation });

                partitionKeyRanges.AddRange(pkRangesResponse);
                pkRangesResponseContinuation = pkRangesResponse.ResponseContinuation;
            }
            while (pkRangesResponseContinuation != null);

            foreach (PartitionKeyRange pkRange in partitionKeyRanges)
            {
                string continuation = null;
                checkpoints.TryGetValue(pkRange.Id, out continuation);

                IDocumentQuery<Document> query = client.CreateDocumentChangeFeedQuery(
                    collectionUri,
                    new ChangeFeedOptions
                    {
                        PartitionKeyRangeId = pkRange.Id,
                        StartFromBeginning = true,
                        RequestContinuation = continuation,
                        MaxItemCount = -1,
                        // Set reading time: only show change feed results modified since StartTime
                        StartTime = DateTime.Now - TimeSpan.FromHours(10)
                    });

                while (query.HasMoreResults)
                {
                    FeedResponse<BasicTask> readChangesResponse = query.ExecuteNextAsync<BasicTask>().Result;

                    foreach (BasicTask changedDocument in readChangesResponse)
                    {
                        Console.WriteLine("\tRead document {0} from the change feed.", changedDocument.TaskId);
                        numChangesRead++;
                    }

                    checkpoints[pkRange.Id] = readChangesResponse.ResponseContinuation;
                }
            }

            Console.WriteLine("Read {0} documents from the change feed", numChangesRead);

            return checkpoints;
        }
    }
}
