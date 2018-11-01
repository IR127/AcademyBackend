namespace AcademyBackend
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AcademyBackend.Concrete_Types;

    class Program
    {
        private static readonly string DatabaseName = "Task";
        private static readonly string CollectionName = "Items";

        public static void Main(string[] args)
        {
            var messagesToSend = ChangeFeed.RunChangeFeedAsync(DatabaseName, CollectionName).GetAwaiter().GetResult();

            using (AzureServiceBus azureServiceBus = new AzureServiceBus(new AdminEmailAlert(new TextFileDataStore())))
            {
                azureServiceBus.SendMessagesAsync(messagesToSend).GetAwaiter().GetResult();
                azureServiceBus.RecieveEmail();
                Console.ReadKey();
            }
        }
    }
}