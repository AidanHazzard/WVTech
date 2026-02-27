using Microsoft.AspNetCore.Identity;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using System.Security.Claims;

namespace MealPlanner.Services
{
    // Defines the contract for all account-related business logic.
    // This allows controllers to depend on abstractions instead of concrete implementations.
    public interface IRegistrationService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterViewModel model);
        Task<User?> FindUserByEmailAsync(string email);
        Task<IdentityResult> ChangePasswordAsync(string email, string newPassword);
        Task<User?> FindUserByClaimAsync(ClaimsPrincipal claim);

        // For registration password reset
        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);

         // For email confirmation

        Task<string> GenerateEmailConfirmationTokenAsync(User user);
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);
        Task<User?> FindUserByIdAsync(string userId);
    }
}
