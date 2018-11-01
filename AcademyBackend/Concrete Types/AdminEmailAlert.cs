using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyBackend.Concrete_Types
{
    using System.Threading.Tasks;
    using AcademyBackend.Interfaces;
    using AcademyBackend.Models;
    using SendGrid;
    using SendGrid.Helpers.Mail;

    class AdminEmailAlert : IMessageAction
    {
        private const string SendGridApiKey = "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";
        private readonly IDataStore dataStore;

        public AdminEmailAlert(IDataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public async Task Excute(ServiceBusMessage message)
        {
            if (!this.IsCompleted(message.Id))
            {
                Console.WriteLine($"Received message with TaskId: {message.Id}.");

                var client = new SendGridClient(SendGridApiKey);
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress("Admin@BestToDoList.com", "BDTL Team"),
                    Subject = "New Task Added",
                    PlainTextContent = $"Task added with Id: {message.Id}"
                };
                msg.AddTo(new EmailAddress("idrees.rabani@asos.com", "Test User"));
                await client.SendEmailAsync(msg);

                this.dataStore.Write(message.Id);
            }
        }

        private bool IsCompleted(string taskId)
        {
            return this.dataStore.Read(taskId);
        }
    }
}
