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
    public class RegisterServiceTests
    {
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<SignInManager<User>> _mockSignInManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private IRegistrationService _registrationService;

        [SetUp]
        public void SetUp()
        {
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null);

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

            _registrationService = new RegistrationService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockRoleManager.Object
            );
        }

        [Test]
        public async Task RegisterUserAsync_ShouldReturnSuccess_WhenRegistrationSucceeds()
        {
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

            var result = await _registrationService.RegisterUserAsync(model);

            Assert.That(result.Succeeded, Is.True);

            _mockUserManager.Verify(u => u.CreateAsync(It.Is<User>(usr => usr.UserName == model.Email), model.Password), Times.Once);
            _mockRoleManager.Verify(r => r.RoleExistsAsync("User"), Times.Once);
            _mockUserManager.Verify(u => u.AddToRoleAsync(It.Is<User>(usr => usr.UserName == model.Email), "User"), Times.Once);
            _mockSignInManager.Verify(s => s.SignInAsync(It.Is<User>(usr => usr.UserName == model.Email), false, null), Times.Once);
        }

        [Test]
        public async Task RegisterUserAsync_ShouldReturnFailed_WhenCreateFails()
        {
            var model = new RegisterViewModel
            {
                Name = "Test User",
                Email = "fail@test.com",
                Password = "Password123!"
            };

            var errors = new IdentityError[] { new IdentityError { Description = "Email already taken" } };

            _mockUserManager
                .Setup(u => u.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var result = await _registrationService.RegisterUserAsync(model);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors, Is.Not.Empty);

            _mockUserManager.Verify(u => u.CreateAsync(It.Is<User>(usr => usr.UserName == model.Email), model.Password), Times.Once);
            _mockRoleManager.Verify(r => r.RoleExistsAsync(It.IsAny<string>()), Times.Never);
            _mockUserManager.Verify(u => u.AddToRoleAsync(It.IsAny<User>(), "User"), Times.Never);
            _mockSignInManager.Verify(s => s.SignInAsync(It.IsAny<User>(), false, null), Times.Never);
        }
    }
}