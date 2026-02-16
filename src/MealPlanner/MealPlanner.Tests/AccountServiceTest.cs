using System.Threading.Tasks;
using MealPlanner.Models;
using MealPlanner.Services;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace MealPlanner.Tests
{
    [TestFixture]
    public class AccountServiceTests
    {
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<SignInManager<User>> _mockSignInManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private IAccountService _accountService;

        [SetUp]
        public void SetUp()
        {
            // Arrange: setup mocks for dependencies
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);

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

            // Arrange: create the AccountService with mocked dependencies
            _accountService = new AccountService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockRoleManager.Object
            );
        }

        // ===================== LOGIN =====================
        [Test]
        public async Task LoginUserAsync_ShouldReturnSuccess_WhenSignInSucceeds()
        {
            // Arrange: create login model and setup mock SignInManager
            var model = new LoginViewModel
            {
                Email = "test@test.com",
                Password = "Password123!",
                RememberMe = false
            };
            _mockSignInManager
                .Setup(s => s.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act: call the service method
            var result = await _accountService.LoginUserAsync(model);

            // Assert: result should indicate success
            Assert.That(result.Succeeded, Is.True);
        }

        // ===================== REGISTER =====================
        [Test]
        public async Task RegisterUserAsync_ShouldReturnSuccess_WhenRegistrationSucceeds()
        {
            // Arrange: create register model and setup mocks for user creation and role assignment
            var model = new RegisterViewModel
            {
                Name = "Test User",
                Email = "test@test.com",
                Password = "Password123!"
            };
            _mockUserManager
                .Setup(u => u.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockRoleManager
                .Setup(r => r.RoleExistsAsync("User"))
                .ReturnsAsync(true);
            _mockUserManager
                .Setup(u => u.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);
            _mockSignInManager
                .Setup(s => s.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);

            // Act: call the service method
            var result = await _accountService.RegisterUserAsync(model);

            // Assert: check success and verify that all relevant calls were made
            Assert.That(result.Succeeded, Is.True);
            _mockUserManager.Verify(u => u.CreateAsync(It.IsAny<User>(), model.Password), Times.Once);
            _mockUserManager.Verify(u => u.AddToRoleAsync(It.IsAny<User>(), "User"), Times.Once);
            _mockSignInManager.Verify(s => s.SignInAsync(It.IsAny<User>(), false, null), Times.Once);
        }

        // ===================== FIND USER =====================
        [Test]
        public async Task FindUserByEmailAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange: create a user and setup UserManager mock
            var email = "found@test.com";
            var user = new User { UserName = email };
            _mockUserManager
                .Setup(u => u.FindByEmailAsync(email))
                .ReturnsAsync(user);

            // Act: call the service method
            var result = await _accountService.FindUserByEmailAsync(email);

            // Assert: user should be returned
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserName, Is.EqualTo(email));
        }

        // ===================== CHANGE PASSWORD =====================
        [Test]
        public async Task ChangePasswordAsync_ShouldReturnFailed_WhenUserNotFound()
        {
            // Arrange: setup UserManager mock to return null (user not found)
            var email = "notfound@test.com";
            var newPassword = "NewPassword123!";
            _mockUserManager
                .Setup(u => u.FindByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act: call the service method
            var result = await _accountService.ChangePasswordAsync(email, newPassword);

            // Assert: result should indicate failure with proper error
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors, Has.One.Matches<IdentityError>(e => e.Description == "User not found"));
        }

        // ===================== LOGOUT =====================
        [Test]
        public async Task LogoutUserAsync_ShouldCallSignOut()
        {
            // Arrange: setup SignInManager mock to verify sign out
            _mockSignInManager
                .Setup(s => s.SignOutAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act: call the service method
            await _accountService.LogoutUserAsync();

            // Assert: SignOutAsync should be called exactly once
            _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
        }
    }
}
