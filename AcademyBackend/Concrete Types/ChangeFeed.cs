namespace AcademyBackend.Concrete_Types
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using AcademyBackend.Models;

    public class ChangeFeed
    {
        private readonly int testInsert;

        private DocumentClient client;

        private readonly string endpointUrl;
        private readonly string authorizationKey;

        public ChangeFeed(string endpointUrl, string authorizationKey, int testInsert = 0)
        {
            this.testInsert = testInsert;
            this.endpointUrl = endpointUrl;
            this.authorizationKey = authorizationKey;
        }

        public async Task<List<ServiceBusMessage>> RunChangeFeedAsync(string databaseId, string collectionId)
        {
            this.client = new DocumentClient(new Uri(this.endpointUrl), this.authorizationKey,
                new ConnectionPolicy {ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp});

            await this.client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });

            DocumentCollection collectionDefinition = new DocumentCollection();
            collectionDefinition.Id = collectionId;
            collectionDefinition.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });
            collectionDefinition.PartitionKey.Paths.Add("/UserId");

            await this.client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseId),
                collectionDefinition,
                new RequestOptions { OfferThroughput = 400 });

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

            if (this.testInsert != 0)
            {
                Console.WriteLine($"Inserting {testInsert} document(s)");
                List<Task> insertTasks = new List<Task>();

                for (int i = 0; i < testInsert; i++)
                {
                    insertTasks.Add(client.CreateDocumentAsync(
                        collectionUri,
                        new BasicTask()
                        {
                            UserId = Guid.NewGuid().ToString(),
                            Added = DateTime.Now,
                            Description = $"Buy {i} apples",
                            IsComplete = false,
                            DueBy = DateTime.Now.AddDays(i * -1),
                            TaskId = Guid.NewGuid()
                        }));
                }

                await Task.WhenAll(insertTasks);
            }

            // Returns all documents in the collection.
            List<ServiceBusMessage> changes = await this.GetChanges(this.client, collectionUri);

            return changes;
        }

        private async Task<List<ServiceBusMessage>> GetChanges(DocumentClient client, Uri collectionUri)
        {
            Console.WriteLine("\n======================================================");
            Console.WriteLine("                 Accessing Change Feed                ");
            Console.WriteLine("======================================================");

            var messagesToSend = new List<ServiceBusMessage>();
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
                IDocumentQuery<Document> query = client.CreateDocumentChangeFeedQuery(
                    collectionUri,
                    new ChangeFeedOptions
                    {
                        PartitionKeyRangeId = pkRange.Id,
                        StartFromBeginning = true,
                        MaxItemCount = -1,
                        StartTime = DateTime.Now - TimeSpan.FromHours(5)
                    });

                while (query.HasMoreResults)
                {
                    FeedResponse<BasicTask> readChangesResponse = query.ExecuteNextAsync<BasicTask>().Result;

                    foreach (BasicTask changedDocument in readChangesResponse)
                    {
                        Console.WriteLine($"Read document {changedDocument.TaskId} from the change feed.");
                        messagesToSend.Add(new ServiceBusMessage() { Id = changedDocument.TaskId.ToString(), DateModified = changedDocument.Added });
                    }
                }
            }
            Console.WriteLine($"\nRead {messagesToSend.Count} document(s) from the change feed");
            return messagesToSend;
        }
    }
}