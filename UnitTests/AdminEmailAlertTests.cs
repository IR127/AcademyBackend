using System;

namespace UnitTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using AcademyBackend.Concrete_Types;
    using AcademyBackend.Interfaces;
    using AcademyBackend.Models;
    using Moq;

    [TestFixture]
    public class AdminEmailAlertTests
    {
        [TestFixture]
        public class Given_A_Valid_Request_To_Process_A_Message
        {
            private Mock<IDataStore> dataStore;
            private Mock<IEmailDeliveryService> emailDeliveryService;
            private AdminEmailAlert adminEmailAlert;
            private Task response;

            [SetUp]
            public void When_Message_Has_Not_Been_Processed()
            {
                // Arrange
                this.dataStore = new Mock<IDataStore>();
                this.dataStore.Setup(x => x.Read(It.IsAny<string>())).Returns(false);

                this.emailDeliveryService = new Mock<IEmailDeliveryService>();
                this.emailDeliveryService.Setup(x => x.Send(It.IsAny<ServiceBusMessage>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

                this.adminEmailAlert = new AdminEmailAlert(this.dataStore.Object, this.emailDeliveryService.Object);

                // Act
                this.response = this.adminEmailAlert.Excute(new ServiceBusMessage() { Id = "1234", DateModified = new DateTimeOffset() });
            }

            [Test]
            public void Then_Check_Request_Has_Not_Been_Completed()
            {
                // Assert
                this.dataStore.Verify(x => x.Read(It.IsAny<string>()), Times.AtLeastOnce );
            }

            [Test]
            public void When_Message_Has_Not_Been_Processed_Then_Send_Email()
            {
                // Assert
                this.emailDeliveryService.Verify(x => x.Send(It.IsAny<ServiceBusMessage>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
            }

            [Test]
            public void When_Message_Has_Not_Been_Processed_Then_Add_Request_To_Datastore()
            {
                // Assert
                this.dataStore.Verify(x => x.Write(It.IsAny<string>()), Times.AtLeastOnce);
            }
        }
        [TestFixture]
        public class Given_A_Invalid_Request_To_Process_A_Message
        {
            [Test]
            public void When_A_Message_Has_Been_Processed_Before_Then_Do_Nothing()
            {
                // Arrange
                var dataStore = new Mock<IDataStore>();
                dataStore.Setup(x => x.Read(It.IsAny<string>())).Returns(true);

                var emailDeliveryService = new Mock<IEmailDeliveryService>();

                var adminEmailAlert = new AdminEmailAlert(dataStore.Object, emailDeliveryService.Object);

                // Act
                var response =
                    adminEmailAlert.Excute(new ServiceBusMessage() { Id = "1234", DateModified = new DateTimeOffset() });

                // Assert
                dataStore.Verify(x => x.Read(It.IsAny<string>()), Times.AtLeastOnce);
                dataStore.Verify(x => x.Write(It.IsAny<string>()), Times.Never);
                emailDeliveryService.Verify(x => x.Send(It.IsAny<ServiceBusMessage>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                Assert.That(response.IsCompleted);
            }
        }
    }
}
