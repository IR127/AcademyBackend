namespace AcademyBackend.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using AcademyBackend.Models;

    public interface IMessageAction
    {
        Task Excute(ServiceBusMessage message);
    }
}
