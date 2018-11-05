namespace AcademyBackend.Concrete_Types
{
    using System;
    using System.Threading.Tasks;
    using AcademyBackend.Interfaces;
    using AcademyBackend.Models;
    using System.Configuration;

    public class AdminEmailAlert : IMessageAction
    {
        private readonly IEmailDeliveryService emailDeliveryService;
        private readonly IDataStore dataStore;

        public AdminEmailAlert(IDataStore dataStore, IEmailDeliveryService emailDeliveryService)
        {
            this.dataStore = dataStore;
            this.emailDeliveryService = emailDeliveryService;
        }

        public async Task Excute(ServiceBusMessage message)
        {
            if (!this.IsCompleted(message.Id))
            {
                Console.WriteLine($"Received message with TaskId: {message.Id}.");

                await this.emailDeliveryService.Send(message);

                this.dataStore.Write(message.Id);
            }
        }

        private bool IsCompleted(string taskId)
        {
            return this.dataStore.Read(taskId);
        }
    }
}
