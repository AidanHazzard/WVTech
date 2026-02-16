using System.Security.Claims;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountService(
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
            return await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
        {
            var user = new User
            {
                FullName = model.Name,
                UserName = model.Email,
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                NormalizedUserName = model.Email.ToUpper(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return result;

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);

            return result;
        }

        public async Task<User?> FindUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> ChangePasswordAsync(string email, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError { Description = "User not found" });
            }

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded) return removeResult;

            return await _userManager.AddPasswordAsync(user, newPassword);
        }

        public async Task<IdentityResult> ResetPasswordAsync(ClaimsPrincipal userPrincipal, string currentPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
                return IdentityResult.Failed(
                    new IdentityError { Description = "User not found" });

            

            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
