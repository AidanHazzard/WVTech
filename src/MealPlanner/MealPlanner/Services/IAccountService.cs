using Microsoft.AspNetCore.Identity;
using MealPlanner.Models;
using MealPlanner.ViewModels;

namespace MealPlanner.Services
{
    // Defines the contract for all account-related business logic.
    // This allows controllers to depend on abstractions instead of concrete implementations.
    public interface IAccountService
    {
        Task<SignInResult> LoginUserAsync(LoginViewModel model);
        Task<IdentityResult> RegisterUserAsync(RegisterViewModel model);
        Task<User?> FindUserByEmailAsync(string email);
        Task<IdentityResult> ChangePasswordAsync(string email, string newPassword);
        Task LogoutUserAsync();
    }
}
