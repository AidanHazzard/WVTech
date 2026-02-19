using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MealPlanner.Services
{
    public interface IAccountSettingsService
    {
        Task<IdentityResult> ChangePasswordAsync(ClaimsPrincipal userPrincipal, string currentPassword, string newPassword);
    }
}
