using System;

namespace UnitTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using AcademyBackend.Concrete_Types;
    using AcademyBackend.Interfaces;
    using AcademyBackend.Models;
    using Moq;

    [TestFixture]
    public class CreateMessagesTests
    {
        [TestFixture]
        public class Given_A_Valid_Request_To_Create_Messages
        {
            private CreateMessages createMessages;
            private DateTimeOffset dateModified;

            [SetUp]
            public void When_Processing_Changes_From_The_Change_Feed()
            {
                // Arrange
                this.createMessages = new CreateMessages();
                this.dateModified = DateTimeOffset.Now;
                var message = new ServiceBusMessage() { Id = "1234", DateModified = this.dateModified };
                var message2 = new ServiceBusMessage() { Id = "2345", DateModified = this.dateModified };

                // Act
                this.createMessages.Add(message);
                this.createMessages.Add(message2);
            }

            [Test]
            public void When_Processing_Changes_From_The_Change_Feed_Then_Add_A_Message()
            {
                // Assert
                Assert.That(this.createMessages.Get()[0].Id, Is.EqualTo("1234"));
                Assert.That(this.createMessages.Get()[0].DateModified, Is.EqualTo(this.dateModified));
            }

            [Test]
            public void When_Requesting_Total_Number_Of_messages_Then_Return_Count()
            {
                // Assert
                Assert.That(this.createMessages.Count(), Is.EqualTo(2));
            }

            [Test]
            public void When_Requesting_All_Message_Then_Retrun_All_Messages()
            {
                // Assert
                Assert.That(this.createMessages.Get()[0].Id, Is.EqualTo("1234"));
                Assert.That(this.createMessages.Get()[1].Id, Is.EqualTo("2345"));
            }
        }
    }
}
