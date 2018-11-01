namespace AcademyBackend.Concrete_Types
{
    using System;
    using System.Threading.Tasks;
    using AcademyBackend.Interfaces;
    using AcademyBackend.Models;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using System.Configuration;

    class AdminEmailAlert : IMessageAction
    {
        private readonly string apiKey;
        private readonly IDataStore dataStore;

        public AdminEmailAlert(IDataStore dataStore, string apiKey)
        {
            this.dataStore = dataStore;
            this.apiKey = apiKey;
        }

        public async Task Excute(ServiceBusMessage message)
        {
            if (!this.IsCompleted(message.Id))
            {
                Console.WriteLine($"Received message with TaskId: {message.Id}.");

                var client = new SendGridClient(this.apiKey);
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
