using System.Threading.Tasks;
using MealPlanner.Services;
using NUnit.Framework;
using Moq;

namespace MealPlanner.Tests
{
    [TestFixture]
    public class EmailServiceTests
    {
        private Mock<IEmailService> _mockEmailService;

        [SetUp]
        public void SetUp()
        {
            _mockEmailService = new Mock<IEmailService>();
        }

        [Test]
        public async Task SendEmailAsync_SendsEmail()
        {
            // Arrange
            var toEmail = "test@example.com";
            var subject = "Hello";
            var body = "Test body";

            _mockEmailService
                .Setup(e => e.SendEmailAsync(toEmail, subject, body))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var code = async () => await _mockEmailService.Object.SendEmailAsync(toEmail, subject, body);

            // Assert
            Assert.That(code, new NUnit.Framework.Constraints.ThrowsNothingConstraint());
            _mockEmailService.Verify(e => e.SendEmailAsync(toEmail, subject, body), Times.Once);
        }
    }
}