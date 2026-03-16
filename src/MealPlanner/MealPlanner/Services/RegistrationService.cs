using System.Security.Claims;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly MealPlannerDBContext _db;

        public RegistrationService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager, MealPlannerDBContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = db;
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
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return result;

            //creates the UserProfile row automatically for every new user
            _db.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                IsDarkTheme = false
            });
            await _db.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            await _userManager.AddToRoleAsync(user, "User");
           
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

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        // already have one of these this needs to be moved to change password
        public async Task<IdentityResult> ResetPasswordAsync(
            User user,
            string token,
            string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }


        public async Task<User?> FindUserByClaimAsync(ClaimsPrincipal claim)
        {
            return await _userManager.GetUserAsync(claim);
        }


        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }


        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<User?> FindUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }



    }
}
