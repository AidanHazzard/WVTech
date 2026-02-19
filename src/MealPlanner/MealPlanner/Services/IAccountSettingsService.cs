using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MealPlanner.Services
{
    public interface IAccountSettingsService
    {
       Task<IdentityResult> ResetPasswordAsync(ClaimsPrincipal userPrincipal, string currentPassword, string newPassword);
    }
}
