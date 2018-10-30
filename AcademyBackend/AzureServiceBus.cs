using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyBackend
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;

    public class AzureServiceBus
    {
        const string ServiceBusConnectionString = "";
        const string TopicName = "";
        static ITopicClient topicClient;

        public AzureServiceBus()
        {
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                topicClient = new TopicClient(ServiceBusConnectionString, TopicName);

                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the topic
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the topic
                    await topicClient.SendAsync(message);
                }

                await topicClient.CloseAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
}
