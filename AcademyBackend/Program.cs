namespace AcademyBackend
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        private static readonly string DatabaseName = "Task";
        private static readonly string CollectionName = "Items";

        public static void Main(string[] args)
        {
            var messagesToSend = ChangeFeed.RunChangeFeedAsync(DatabaseName, CollectionName).GetAwaiter().GetResult();

            using (AzureServiceBus azureServiceBus = new AzureServiceBus())
            {
                azureServiceBus.SendMessagesAsync(messagesToSend).GetAwaiter().GetResult();
                azureServiceBus.RecieveEmail();
                Console.ReadKey();
            }
        }
    }
}