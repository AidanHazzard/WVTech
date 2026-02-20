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
    public class LoginControllerTests
    {
        private Mock<ILoginService> _mockAccountService;
        private LoginController _controller;

        [SetUp]
        public void SetUp()
        {
            // Arrange: create mock and controller
            _mockAccountService = new Mock<ILoginService>();
            _controller = new LoginController(_mockAccountService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        // ===================== LOGIN =====================
        [Test]
        public void Login_Get_ReturnsView()
        {
            // Act
            var result = _controller.Login();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Login_Post_ReturnsView_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");
            var model = new LoginViewModel();

            // Act
            var result = await _controller.Login(model);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Login_Post_RedirectsToHome_WhenLoginSucceeds()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@test.com",
                Password = "Password123",
                RememberMe = false
            };
            _mockAccountService.Setup(s => s.LoginUserAsync(model))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Login_Post_ReturnsView_WhenLoginFails()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@test.com",
                Password = "Password123",
                RememberMe = false
            };
            _mockAccountService.Setup(s => s.LoginUserAsync(model))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ModelState.ContainsKey(""), Is.True);
        }

        // ===================== LOGOUT =====================
        [Test]
        public async Task Logout_Post_RedirectsToHome()
        {
            // Arrange
            _mockAccountService.Setup(s => s.LogoutUserAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Home"));
        }
    }
}
