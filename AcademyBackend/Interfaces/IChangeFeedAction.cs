namespace AcademyBackend.Interfaces
{
    using System.Collections.Generic;
    using AcademyBackend.Models;

    public interface IChangeFeedAction
    {
        void Add(ServiceBusMessage messageToSend);

        int Count();
        List<ServiceBusMessage> Get();
    }
}