using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyBackend.Concrete_Types
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using AcademyBackend.Models;

    public class ChangeFeed
    {
        private static readonly int numberToInsert = 10;

        private static DocumentClient client;

        private static readonly string endpointUrl = "https://todo-list.documents.azure.com:443";
        private static readonly string authorizationKey = "EAuHmYP4pDOvXoDDS9oU1HlZ7wGI0beCQXaKYxOaz6LNsAh60w6cnYcfY33xDxRJM23puYFDvZrDOR18ou59FQ==";

        public static async Task<List<ServiceBusMessage>> RunChangeFeedAsync(string databaseId, string collectionId)
        {
            client = new DocumentClient(new Uri(endpointUrl), authorizationKey,
                new ConnectionPolicy {ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp});

            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });

            DocumentCollection collectionDefinition = new DocumentCollection();
            collectionDefinition.Id = collectionId;
            collectionDefinition.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });
            collectionDefinition.PartitionKey.Paths.Add("/UserId");

            await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseId),
                collectionDefinition,
                new RequestOptions { OfferThroughput = 400 });

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

            if (numberToInsert != 0)
            {
                Console.WriteLine($"Inserting {numberToInsert} document(s)");
                List<Task> insertTasks = new List<Task>();

                for (int i = 0; i < numberToInsert; i++)
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
            List<ServiceBusMessage> changes = await GetChanges(client, collectionUri);

            return changes;
        }

        private static async Task<List<ServiceBusMessage>> GetChanges(DocumentClient client, Uri collectionUri)
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