using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using AcademyBackend.Concrete_Types;
    using AcademyBackend.Interfaces;
    using AcademyBackend.Models;
    using Moq;
    using NUnit.Framework;
    using SendGrid;
    using SendGrid.Helpers.Mail;

    [TestFixture]
    class SendGridTests
    {
        [TestFixture]
        public class Given_A_Valid_Request_To_Send_An_Email
        {
            [Test]
            public async Task When_Using_SendGrid_Then_Send_Email()
            {
                // Arrange
                var sendGridClient = new Mock<ISendGridClient>();
                sendGridClient.Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync((Response)null);

                var message = new ServiceBusMessage(){Id = "1234", DateModified = DateTimeOffset.Now};

                var sendGrid = new SendGrid(sendGridClient.Object);

                // Act
                await sendGrid.Send(message);

                // Assert
                sendGridClient.Verify(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }
            [Test]
            public async Task When_Using_SendGrid_Then_Create_An_Email()
            {
                // Arrange
                var sendGridClient = new Mock<ISendGridClient>();
                sendGridClient.Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync((Response)null);

                var message = new ServiceBusMessage() { Id = "1234", DateModified = DateTimeOffset.Now };

                var sendGrid = new SendGrid(sendGridClient.Object);

                // Act
                await sendGrid.Send(message);

                // Assert
                sendGridClient.Verify(x => x.SendEmailAsync(It.Is<SendGridMessage>(y => y.PlainTextContent == $"Task added with Id: {message.Id}"), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }
        }
    }
}
