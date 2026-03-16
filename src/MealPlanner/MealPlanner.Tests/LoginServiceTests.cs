using System.Threading.Tasks;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests
{
    [TestFixture]
    public class LoginServiceTests
    {
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<SignInManager<User>> _mockSignInManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<ILogger<LoginService>> _mockLogger;
        private LoginService _loginService;

        [SetUp]
        public void SetUp()
        {
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null
            );

            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();

            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null
            );

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null
            );

            _mockLogger = new Mock<ILogger<LoginService>>();

            _loginService = new LoginService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockRoleManager.Object,
                _mockLogger.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _loginService = null;
            _mockUserManager = null;
            _mockSignInManager = null;
            _mockRoleManager = null;
            _mockLogger = null;
        }

        // ===================== LOGIN =====================
        [Test]
        public async Task LoginUserAsync_ReturnsFailed_WhenUserNotFound()
        {
            _mockUserManager.Setup(u => u.FindByEmailAsync("test@test.com"))
                .ReturnsAsync((User)null);

            var model = new LoginViewModel
            {
                Email = "test@test.com",
                Password = "Password123",
                RememberMe = false
            };

            var result = await _loginService.LoginUserAsync(model);

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task LoginUserAsync_ReturnsNotAllowed_WhenEmailNotConfirmed()
        {
            var user = new User { EmailConfirmed = false };
            _mockUserManager.Setup(u => u.FindByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            var model = new LoginViewModel
            {
                Email = "test@test.com",
                Password = "Password123",
                RememberMe = false
            };

            var result = await _loginService.LoginUserAsync(model);

            Assert.That(result.IsNotAllowed, Is.True);
        }

        [Test]
        public async Task LoginUserAsync_ReturnsSuccess_WhenSignInSucceeds()
        {
            var user = new User { EmailConfirmed = true };
            _mockUserManager.Setup(u => u.FindByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            _mockSignInManager.Setup(s =>
                s.PasswordSignInAsync("test@test.com", "Password123", false, false))
                .ReturnsAsync(SignInResult.Success);

            var model = new LoginViewModel
            {
                Email = "test@test.com",
                Password = "Password123",
                RememberMe = false
            };

            var result = await _loginService.LoginUserAsync(model);

            Assert.That(result.Succeeded, Is.True);
            _mockSignInManager.Verify(s =>
                s.PasswordSignInAsync("test@test.com", "Password123", false, false),
                Times.Once);
        }

        // ===================== REMEMBER ME =====================
        [Test]
        public async Task LoginUserAsync_WithRememberMeTrue_CallsSignInWithPersistentCookie()
        {
            var user = new User { EmailConfirmed = true };
            _mockUserManager.Setup(u => u.FindByEmailAsync("user@test.com"))
                .ReturnsAsync(user);

            _mockSignInManager.Setup(s =>
                s.PasswordSignInAsync("user@test.com", "Password123", true, false))
                .ReturnsAsync(SignInResult.Success);

            var model = new LoginViewModel
            {
                Email = "user@test.com",
                Password = "Password123",
                RememberMe = true
            };

            var result = await _loginService.LoginUserAsync(model);

            Assert.That(result.Succeeded, Is.True);
            _mockSignInManager.Verify(s =>
                s.PasswordSignInAsync("user@test.com", "Password123", true, false),
                Times.Once);
        }

        [Test]
        public async Task LoginUserAsync_WithRememberMeFalse_CallsSignInWithSessionCookie()
        {
            var user = new User { EmailConfirmed = true };
            _mockUserManager.Setup(u => u.FindByEmailAsync("user@test.com"))
                .ReturnsAsync(user);

            _mockSignInManager.Setup(s =>
                s.PasswordSignInAsync("user@test.com", "Password123", false, false))
                .ReturnsAsync(SignInResult.Success);

            var model = new LoginViewModel
            {
                Email = "user@test.com",
                Password = "Password123",
                RememberMe = false
            };

            var result = await _loginService.LoginUserAsync(model);

            Assert.That(result.Succeeded, Is.True);
            _mockSignInManager.Verify(s =>
                s.PasswordSignInAsync("user@test.com", "Password123", false, false),
                Times.Once);
        }

        // ===================== LOGOUT =====================
        [Test]
        public async Task LogoutUserAsync_CallsSignOut()
        {
            _mockSignInManager.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

            await _loginService.LogoutUserAsync();

            _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
        }
    }
}