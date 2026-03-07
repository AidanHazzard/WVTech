using System.Security.Claims;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Services
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public LoginService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<SignInResult> LoginUserAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return SignInResult.Failed;
            }

            if (!user.EmailConfirmed)
            {
                return SignInResult.NotAllowed;
            }
            return await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
