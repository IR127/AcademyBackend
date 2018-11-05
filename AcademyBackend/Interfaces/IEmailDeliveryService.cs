namespace AcademyBackend.Interfaces
{
    using System.Threading.Tasks;
    using AcademyBackend.Models;

    public interface IEmailDeliveryService
    {
        Task Send(ServiceBusMessage message, string emailFrom = "Admin@BestToDoList.com", string emailTo = "idrees.rabani@asos.com");
    }
}