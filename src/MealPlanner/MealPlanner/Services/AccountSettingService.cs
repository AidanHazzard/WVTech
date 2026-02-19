using Microsoft.AspNetCore.Identity;
using MealPlanner.Models;
using System.Security.Claims;

namespace MealPlanner.Services
{
    public class AccountSettingsService : IAccountSettingsService
    {
        private readonly UserManager<User> _userManager;

        public AccountSettingsService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> ChangePasswordAsync(
            ClaimsPrincipal userPrincipal, string currentPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }
    }
}
