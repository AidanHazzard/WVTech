using Microsoft.AspNetCore.Identity;
using MealPlanner.Models;
using MealPlanner.ViewModels;

namespace MealPlanner.Services;

public class AccountService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountService(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    // Handles user registration including role creation and assignment
    public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
    {
        var user = new User
        {
            FullName = model.Name,
            UserName = model.Email,
            NormalizedUserName = model.Email.ToUpper(),
            Email = model.Email,
            NormalizedEmail = model.Email.ToUpper(),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) return result;

        // Ensure "User" role exists
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Assign role
        await _userManager.AddToRoleAsync(user, "User");

        // Sign in the user immediately
        await _signInManager.SignInAsync(user, isPersistent: false);

        return result;
    }

    // Checks user credentials and signs in
    public async Task<SignInResult> LoginUserAsync(LoginViewModel model)
    {
        return await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
    }

    // Verify if user exists by email
    public async Task<User?> FindUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    // Change password workflow
    public async Task<IdentityResult> ChangePasswordAsync(string email, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded) return removeResult;

        var addResult = await _userManager.AddPasswordAsync(user, newPassword);
        return addResult;
    }

    // Logout user
    public async Task LogoutUserAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
