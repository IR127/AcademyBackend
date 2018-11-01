namespace AcademyBackend.Concrete_Types
{
    using Microsoft.Azure.ServiceBus;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AcademyBackend.Interfaces;
    using AcademyBackend.Models;
    using Newtonsoft.Json;
    using SendGrid;
    using SendGrid.Helpers.Mail;

    public class AzureServiceBus : IDisposable
    {
        private readonly IMessageAction messageAction;
        const string ServiceBusConnectionString = "Endpoint=sb://besttodolist.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=rkmA9KEJvxzS8f7Stw/eDDmvX2e0QNBbQllTniH7Emw=";
        const string TopicName = "todo-events";
        const string SubscriptionName = "SendGrid";
        static ITopicClient topicClient;
        static ISubscriptionClient subscriptionClient;

        public AzureServiceBus(IMessageAction messageAction)
        {
            this.messageAction = messageAction;
        }

        public async Task SendMessagesAsync(IList<ServiceBusMessage> messagesToSend)
        {
            Console.WriteLine("\n======================================================");
            Console.WriteLine("        Sending Messages To Azure Service Bus         ");
            Console.WriteLine("======================================================");

            var numberOfMessagesToSend = messagesToSend.Count;

            topicClient = new TopicClient(ServiceBusConnectionString, TopicName);

            for (var i = 0; i < numberOfMessagesToSend; i++)
            {
                string messageBody = JsonConvert.SerializeObject(messagesToSend[i]);
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                Console.WriteLine($"Sent message {messagesToSend[i].Id} to Azure Service Bus (Topic: {TopicName} | Subscription: {SubscriptionName})");

                await topicClient.SendAsync(message);
            }

            await topicClient.CloseAsync();
        }

        public void RecieveEmail()
        {
            Console.WriteLine("\n======================================================");
            Console.WriteLine("      Processing Messages From Azure Service Bus      ");
            Console.WriteLine("======================================================");

            subscriptionClient = new SubscriptionClient(ServiceBusConnectionString, TopicName, SubscriptionName);

            this.ReceiveMessages();
        }

        private void ReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(this.ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            subscriptionClient.RegisterMessageHandler(this.ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var messageJson = Encoding.UTF8.GetString(message.Body);
            var updateMessage = JsonConvert.DeserializeObject<ServiceBusMessage>(messageJson);

            await this.messageAction.Excute(updateMessage);

            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            subscriptionClient.CloseAsync();
        }
    }
}
