using System.Threading.Tasks;
using MealPlanner.Controllers;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MealPlanner.Tests
{
    [TestFixture]
    public class LoginControllerTests
    {
        private Mock<ILoginService> _mockLoginService;
        private Mock<ILogger<LoginController>> _mockLogger;
        private LoginController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockLoginService = new Mock<ILoginService>();
            _mockLogger = new Mock<ILogger<LoginController>>();

            _controller = new LoginController(
                _mockLoginService.Object,
                _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        // ===================== LOGIN (GET) =====================

        [Test]
        public void Login_Get_ReturnsView()
        {
            var result = _controller.Login();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ===================== LOGIN (POST) =====================

        [Test]
        public async Task Login_Post_ReturnsView_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("Email", "Required");

            var result = await _controller.Login(new LoginViewModel());

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Login_Post_RedirectsToHome_WhenLoginSucceeds()
        {
            var model = new LoginViewModel
            {
                Email = "test@test.com",
                Password = "Password123",
                RememberMe = false
            };

            _mockLoginService
                .Setup(s => s.LoginUserAsync(It.IsAny<LoginViewModel>()))
                .ReturnsAsync(IdentitySignInResult.Success);

            var result = await _controller.Login(model);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect.ActionName, Is.EqualTo("Index"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Login_Post_ReturnsView_WhenLoginFails()
        {
            _mockLoginService
                .Setup(s => s.LoginUserAsync(It.IsAny<LoginViewModel>()))
                .ReturnsAsync(IdentitySignInResult.Failed);

            var result = await _controller.Login(new LoginViewModel());

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ModelState.ContainsKey(""), Is.True);
        }

        // ===================== REMEMBER ME =====================

        [Test]
        public async Task Login_Post_PassesRememberMeTrue_ToLoginService()
        {
            var model = new LoginViewModel
            {
                Email = "test@test.com",
                Password = "Password123",
                RememberMe = true
            };

            _mockLoginService
                .Setup(s => s.LoginUserAsync(It.Is<LoginViewModel>(m => m.RememberMe)))
                .ReturnsAsync(IdentitySignInResult.Success)
                .Verifiable();

            await _controller.Login(model);

            _mockLoginService.Verify();
        }

        [Test]
        public async Task Login_Post_PassesRememberMeFalse_ToLoginService()
        {
            var model = new LoginViewModel
            {
                Email = "test@test.com",
                Password = "Password123",
                RememberMe = false
            };

            _mockLoginService
                .Setup(s => s.LoginUserAsync(It.Is<LoginViewModel>(m => !m.RememberMe)))
                .ReturnsAsync(IdentitySignInResult.Success)
                .Verifiable();

            await _controller.Login(model);

            _mockLoginService.Verify();
        }

        // ===================== LOGOUT =====================

        [Test]
        public async Task Logout_Post_RedirectsToHome()
        {
            _mockLoginService
                .Setup(s => s.LogoutUserAsync())
                .Returns(Task.CompletedTask);

            var result = await _controller.Logout();

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect.ActionName, Is.EqualTo("Index"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Home"));
        }
    }
}