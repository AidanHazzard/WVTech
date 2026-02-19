using System.Threading.Tasks;
using MealPlanner.Controllers;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests
{
    [TestFixture]
    public class RegisterControllerTests
    {
        private Mock<IRegistrationService> _mockAccountService;
        private RegisterController _controller;

        [SetUp]
        public void SetUp()
        {
            // Arrange: create mock and controller
            _mockAccountService = new Mock<IRegistrationService>();
            _controller = new RegisterController(_mockAccountService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        // ===================== REGISTER =====================
        [Test]
        public async Task Register_Post_RedirectsToHome_WhenRegistrationSucceeds()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Name = "Test User",
                Email = "test@test.com",
                Password = "Password123!"
            };
            _mockAccountService.Setup(s => s.RegisterUserAsync(model))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Register_Post_ReturnsView_WhenRegistrationFails()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Name = "Test User",
                Email = "fail@test.com",
                Password = "Password123!"
            };
            var errors = new IdentityError[] { new IdentityError { Description = "Email already taken" } };
            _mockAccountService.Setup(s => s.RegisterUserAsync(model))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _controller.Register(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ModelState.ContainsKey(""), Is.True);
        }

        // ===================== VERIFY EMAIL =====================
        [Test]
        public async Task VerifyEmail_Post_RedirectsToChangePassword_WhenUserFound()
        {
            // Arrange
            var model = new VerifyEmailViewModel { Email = "found@test.com" };
            var user = new User { UserName = "found@test.com" };
            _mockAccountService.Setup(s => s.FindUserByEmailAsync(model.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.VerifyEmail(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("ChangePassword"));
            Assert.That(redirectResult.RouteValues["username"], Is.EqualTo(user.UserName));
        }

        [Test]
        public async Task VerifyEmail_Post_ReturnsView_WhenUserNotFound()
        {
            // Arrange
            var model = new VerifyEmailViewModel { Email = "notfound@test.com" };
            _mockAccountService.Setup(s => s.FindUserByEmailAsync(model.Email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.VerifyEmail(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ModelState.ContainsKey(""), Is.True);
        }

        // ===================== CHANGE PASSWORD =====================
        [Test]
        public async Task ChangePassword_Post_RedirectsToLogin_WhenChangeSucceeds()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                Email = "user@test.com",
                NewPassword = "NewPassword123!"
            };
            _mockAccountService.Setup(s => s.ChangePasswordAsync(model.Email, model.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));
        }

        [Test]
        public async Task ChangePassword_Post_ReturnsView_WhenChangeFails()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                Email = "user@test.com",
                NewPassword = "NewPassword123!"
            };
            var errors = new IdentityError[] { new IdentityError { Description = "Password invalid" } };
            _mockAccountService.Setup(s => s.ChangePasswordAsync(model.Email, model.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ModelState.ContainsKey(""), Is.True);
        }

     
    }
}
