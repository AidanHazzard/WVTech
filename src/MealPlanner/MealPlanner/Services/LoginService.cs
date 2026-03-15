using System.Security.Claims;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Services
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
         private readonly ILogger<LoginService> _logger;


        public LoginService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager
            , ILogger<LoginService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

          public async Task<SignInResult> LoginUserAsync(LoginViewModel model)
        {
            _logger.LogInformation("--------------------Login attempt for {Email}. RememberMe: {RememberMe}---------------------", model.Email, model.RememberMe);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed. User not found: {Email}", model.Email);
                return SignInResult.Failed;
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Login blocked. Email not confirmed: {Email}", model.Email);
                return SignInResult.NotAllowed;
            }

            _logger.LogInformation("--------------------Calling PasswordSignInAsync. Persistent cookie: {RememberMe}---------------------", model.RememberMe);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            _logger.LogInformation("--------------------Login result for {Email}: {Result}---------------------", model.Email, result.ToString());

            return result;
        }


        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
