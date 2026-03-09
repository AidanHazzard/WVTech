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
    public class LoginServiceTests
    {
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<SignInManager<User>> _mockSignInManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private ILoginService _loginService;

        [SetUp]
        public void SetUp()
        {
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

            _loginService = new LoginService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockRoleManager.Object
            );
        }

        [Test]
        public async Task LoginUserAsync_ShouldReturnSuccess_WhenSignInSucceeds()
        {
            var model = new LoginViewModel
            {
                Email = "test@test.com",
                Password = "Password123!",
                RememberMe = false
            };

            _mockSignInManager
                .Setup(s => s.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false))
                .ReturnsAsync(SignInResult.Success);

            var result = await _loginService.LoginUserAsync(model);

            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public async Task LogoutUserAsync_ShouldCallSignOut()
        {
            _mockSignInManager
                .Setup(s => s.SignOutAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _loginService.LogoutUserAsync();

            _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
        }

         // ======= NEW REMEMBER ME TESTS =======

        [Test]
        public async Task LoginUserAsync_WithRememberMeFalse_CallsSignInWithFalse()
        {
            var model = new LoginViewModel
            {
                Email = "user@test.com",
                Password = "Password123",
                RememberMe = false
            };

            _mockSignInManager
                .Setup(s => s.PasswordSignInAsync(model.Email, model.Password, false, false))
                .ReturnsAsync(SignInResult.Success)
                .Verifiable();

            var result = await _loginService.LoginUserAsync(model);

            Assert.That(result.Succeeded, Is.True);
            _mockSignInManager.Verify();
        }

        [Test]
        public async Task LoginUserAsync_WithRememberMeTrue_CallsSignInWithTrue()
        {
            var model = new LoginViewModel
            {
                Email = "user@test.com",
                Password = "Password123",
                RememberMe = true
            };

            _mockSignInManager
                .Setup(s => s.PasswordSignInAsync(model.Email, model.Password, true, false))
                .ReturnsAsync(SignInResult.Success)
                .Verifiable();

            var result = await _loginService.LoginUserAsync(model);

            Assert.That(result.Succeeded, Is.True);
            _mockSignInManager.Verify();
        }
    }
}
