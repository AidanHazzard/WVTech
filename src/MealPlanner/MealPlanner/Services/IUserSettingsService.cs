using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MealPlanner.Services
{
    public interface IUserSettingsService
    {
       Task<IdentityResult> ResetPasswordAsync(ClaimsPrincipal userPrincipal, string currentPassword, string newPassword);
    }
}
