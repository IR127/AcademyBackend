using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyBackend.Concrete_Types
{
    using System.Threading.Tasks;
    using AcademyBackend.Interfaces;
    using AcademyBackend.Models;
    using global::SendGrid;
    using global::SendGrid.Helpers.Mail;

    public class SendGrid : IEmailDeliveryService
    {
        private readonly ISendGridClient sendGridClient;

        public SendGrid(ISendGridClient sendGridClient)
        {
            this.sendGridClient = sendGridClient;
        }
        
        public async Task Send(ServiceBusMessage message, string emailFrom = "Admin@BestToDoList.com", string emailTo = "idrees.rabani@asos.com")
        {
            var emailToSend = this.CreateEmail(message, emailFrom, emailTo);
            await this.sendGridClient.SendEmailAsync(emailToSend);
        }

        private SendGridMessage CreateEmail(ServiceBusMessage message, string emailFrom, string emailTo)
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(emailFrom),
                Subject = "New Task Added",
                PlainTextContent = $"Task added with Id: {message.Id}"
            };
            msg.AddTo(new EmailAddress(emailTo));
            return msg;
        }
    }
}
