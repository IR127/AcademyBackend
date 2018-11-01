namespace AcademyBackend.Models
{
    using System;

    public class ServiceBusMessage
    {
        public string Id { get; set; }
        public DateTimeOffset DateModified { get; set; }
    }
}
