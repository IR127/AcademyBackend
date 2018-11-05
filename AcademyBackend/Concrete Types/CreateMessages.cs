using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyBackend.Concrete_Types
{
    using AcademyBackend.Interfaces;
    using AcademyBackend.Models;

    public class CreateMessages : IChangeFeedAction
    {
        private readonly List<ServiceBusMessage> messagesToSend = new List<ServiceBusMessage>();

        public void Add(ServiceBusMessage messageToSend)
        {
            this.messagesToSend.Add(messageToSend);
        }

        public int Count()
        {
            return this.messagesToSend.Count;
        }

        public List<ServiceBusMessage> Get()
        {
            return this.messagesToSend;
        }
    }
}
