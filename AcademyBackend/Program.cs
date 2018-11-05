namespace AcademyBackend
{
    using System;
    using System.Configuration;
    using System.IO;
    using AcademyBackend.Concrete_Types;
    using Microsoft.Extensions.Configuration;
    using SendGrid;

    class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var changeFeed = new ChangeFeed(new CreateMessages(), config["Cosmos:EndpointUrl"], config["Cosmos:AuthorizationKey"], 1);

            var messagesToSend = changeFeed.RunChangeFeedAsync(config["Cosmos:DatabaseName"], config["Cosmos:CollectionName"], config["Cosmos:PartitionKey"]).GetAwaiter().GetResult();

            using (AzureServiceBus azureServiceBus = new AzureServiceBus(
                new AdminEmailAlert(new TextFileDataStore(), new SendGrid(new SendGridClient(config["SendGrid:ApiKey"]))),
                config["AzureServiceBus:ConnectionString"],
                config["AzureServiceBus:TopicName"],
                config["AzureServiceBus:SubscriptionName"]))
            {
                azureServiceBus.SendMessagesAsync(messagesToSend).GetAwaiter().GetResult();
                azureServiceBus.RecieveEmail();
                Console.ReadKey();
            }
        }
    }
}