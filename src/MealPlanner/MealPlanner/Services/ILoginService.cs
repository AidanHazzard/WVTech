using Microsoft.AspNetCore.Identity;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using System.Security.Claims;

namespace MealPlanner.Services
{
    // Defines the contract for all account-related business logic.
    // This allows controllers to depend on abstractions instead of concrete implementations.
    public interface ILoginService
    {
        Task<SignInResult> LoginUserAsync(LoginViewModel model);
        Task LogoutUserAsync();
    }
}
